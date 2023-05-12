using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using AGSUnpacker.Graphics.Formats;
using AGSUnpacker.Shared;
using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Graphics
{
  internal struct PngHeader
  {
    public uint Width { get; }
    public uint Height { get; }
    public byte BitDepth { get; }
    public byte ColorType { get; }
    public byte CompressionMethod { get; }
    public byte FilterMethod { get; }
    public byte InterlaceMethod { get; }

    public long Pitch => Width * BytesPerSample * SamplesCount;

    public int BytesPerSample => (int)Math.Ceiling(BitDepth / 8.0);

    public int SamplesCount
    {
      get
      {
        switch ((ColorTypes)ColorType)
        {
          case ColorTypes.Indexed:
          case ColorTypes.Greyscale:
            return 1;

          case ColorTypes.GrayscaleAlpha:
            return 2;

          case ColorTypes.Truecolor:
            return 3;

          case ColorTypes.TruecolorAlpha:
            return 4;

          default:
            throw new InvalidDataException(
              $"PNG: Can't get bytes per pixel for color type: {ColorType}");
        }
      }
    }

    public PngHeader(uint width, uint height, byte bitDepth, byte colorType,
      byte compressionMethod, byte filterMethod, byte interlaceMethod)
    {
      Width = width;
      Height = height;
      BitDepth = bitDepth;
      ColorType = colorType;
      CompressionMethod = compressionMethod;
      FilterMethod = filterMethod;
      InterlaceMethod = interlaceMethod;
    }

    internal enum ColorTypes
    {
      Greyscale = 0,
      Truecolor = 2,
      Indexed = 3,
      GrayscaleAlpha = 4,
      TruecolorAlpha = 6,
    }
  }

  internal abstract class PngChunkReader
  {
    internal static PngChunkReader Create(string type)
    {
      switch (type)
      {
        case PngChunk.Types.IHDR:
          return new PngChunkIHDR();

        case PngChunk.Types.PLTE:
          return new PngChunkPLTE();

        case PngChunk.Types.IDAT:
          return new PngChunkIDAT();

        case PngChunk.Types.tRNS:
          return new PngChunkTRNS();

        default:
          return new PngChunkUnknown();
      }
    }

    internal abstract void Read(PngImage image, BinaryReader reader);
  }

  internal class PngChunkIHDR : PngChunkReader
  {
    internal override void Read(PngImage image, BinaryReader reader)
    {
      if (image.Header != null)
        throw new InvalidDataException("PNG: More than one header chunk detected.");

      uint width = reader.ReadUInt32BE();
      if (width == 0)
        throw new InvalidDataException("PNG: Image width cannot be zero.");

      uint height = reader.ReadUInt32BE();
      if (height == 0)
        throw new InvalidDataException("PNG: Image height cannot be zero.");

      byte bitDepth = reader.ReadByte();
      if (!IsValidBitDepth(bitDepth))
        throw new InvalidDataException($"PNG: Invalid bit depth value: {bitDepth}.");

      byte colorType = reader.ReadByte();
      if (!IsValidColorType(colorType, bitDepth))
        throw new InvalidDataException($"PNG: Invalid color type '{colorType}' for a bit depth '{bitDepth}'.");

      byte compressionMethod = reader.ReadByte();
      if (compressionMethod != 0)
        throw new NotSupportedException(
          $"PNG: Unsupported compression method: {compressionMethod}");

      byte filterMethod = reader.ReadByte();
      if (filterMethod != 0)
        throw new NotSupportedException(
          $"PNG: Unsupported filter method: {filterMethod}");

      byte interlaceMethod = reader.ReadByte();

      image.Header = new PngHeader(
        width, height, bitDepth, colorType,
        compressionMethod,
        filterMethod,
        interlaceMethod
      );
    }

    private bool IsValidBitDepth(byte bitDepth)
    {
      switch (bitDepth)
      {
        case 1:
        case 2:
        case 4:
        case 8:
          return true;

        case 16:
          throw new NotSupportedException("PNG: 16-bit depth is not supported.");

        default:
          return false;
      }
    }

    private bool IsValidColorType(byte colorType, byte bitDepth)
    {
      switch ((PngHeader.ColorTypes)colorType)
      {
        case PngHeader.ColorTypes.Greyscale:
          return true;

        case PngHeader.ColorTypes.Truecolor:
        case PngHeader.ColorTypes.GrayscaleAlpha:
        case PngHeader.ColorTypes.TruecolorAlpha:
          switch (bitDepth)
          {
            case 8:
              return true;

            case 16:
              throw new NotSupportedException("PNG: 16-bit depth is not supported.");

            default:
              return false;
          }

        case PngHeader.ColorTypes.Indexed:
          switch (bitDepth)
          {
            case 1:
            case 2:
            case 4:
            case 8:
              return true;

            default:
              return false;
          }

        default:
          return false;
      }
    }
  }

  internal class PngChunkPLTE : PngChunkReader
  {
    internal override void Read(PngImage image, BinaryReader reader)
    {
      if (image.Header == null)
        throw new InvalidDataException("PNG: Header chunk must precede palette chunk.");

      if (image.PaletteColors != null)
        throw new InvalidDataException("PNG: More than one palette chunk detected.");

      if (image.Header?.ColorType == (byte)PngHeader.ColorTypes.Greyscale
        || image.Header?.ColorType == (byte)PngHeader.ColorTypes.GrayscaleAlpha)
        throw new InvalidDataException("PNG: Palette chunk cannot appear for a grayscale image.");

      if (reader.BaseStream.Length % 3 != 0)
        throw new InvalidDataException(
          $"PNG: Palette chunk length must be divisible by 3, got: {reader.BaseStream.Length}.");

      long count = reader.BaseStream.Length / 3;
      if (count > 256)
        throw new InvalidDataException($"PNG: Too many palette entries: {count}.");

      image.PaletteColors = new Color[count];
      for (int i = 0; i < image.PaletteColors.Length; ++i)
      {
        byte red = reader.ReadByte();
        byte green = reader.ReadByte();
        byte blue = reader.ReadByte();

        image.PaletteColors[i] = new Color(red, green, blue);
      }
    }
  }

  internal class PngChunkIDAT : PngChunkReader
  {
    internal override void Read(PngImage image, BinaryReader reader)
    {
      reader.BaseStream.CopyTo(image.ImageDataStream);
    }
  }

  internal class PngChunkTRNS : PngChunkReader
  {
    internal override void Read(PngImage image, BinaryReader reader)
    {
      if ((PngHeader.ColorTypes)image.Header?.ColorType != PngHeader.ColorTypes.Indexed)
        throw new NotSupportedException("PNG: Transparency chunk only supported for indexed color type.");

      if (image.PaletteColors == null)
        throw new InvalidDataException("PNG: Transparency chunk must come after palette chunk.");

      if (reader.BaseStream.Length > image.PaletteColors.Length)
        throw new InvalidDataException(
          $"PNG: Transparency chunk length is larger than palette colors: {reader.BaseStream.Length}");

      int count = (int)reader.BaseStream.Length;
      for (int i = 0; i < count; ++i)
        image.PaletteColors[i].A = reader.ReadByte();
    }
  }

  internal class PngChunkUnknown : PngChunkReader
  {
    internal override void Read(PngImage image, BinaryReader reader)
    {
      // NOTE(adm244): just ignore unknown chunks
    }
  }

  internal struct PngChunk
  {
    internal string Type { get; private set; }

    internal static PngChunk Read(PngImage image, BinaryReader reader)
    {
      PngChunk chunk = new PngChunk();

      long length = reader.ReadUInt32BE();
      chunk.Type = reader.ReadFixedCString(4);

      if (length > 0)
      {
        long position = reader.BaseStream.Position;

        using ReadOnlySubStream stream = new ReadOnlySubStream(
          reader.BaseStream,
          reader.BaseStream.Position,
          length
        );

        using BinaryReader streamReader = new BinaryReader(stream);

        PngChunkReader chunkReader = PngChunkReader.Create(chunk.Type);
        chunkReader.Read(image, streamReader);

        reader.BaseStream.Seek(position + length, SeekOrigin.Begin);
      }

      // TODO(adm244): check if crc is correct (includes type and data fields)
      uint crc32 = reader.ReadUInt32BE();

      return chunk;
    }

    internal sealed class Types
    {
      // "Critical" chunks
      internal const string IHDR = "IHDR";
      internal const string PLTE = "PLTE";
      internal const string IDAT = "IDAT";
      internal const string IEND = "IEND";

      // "Ancillary" chunks
      internal const string tRNS = "tRNS";

      private Types() { }
    }
  }

  internal abstract class PngPipeline
  {
    internal protected PngHeader Header { get; }

    internal PngPipeline(PngHeader header)
    {
      Header = header;
    }

    internal abstract byte[] Decode(Stream input);
  }

  internal class PngPipelineManager
  {
    private IList<PngPipeline> Pipelines { get; set; }

    private PngPipelineManager()
    {
    }

    internal static PngPipelineManager Create(PngImage image)
    {
      if (image.Header == null)
        throw new InvalidDataException("PNG: Null image header detected.");

      PngHeader header = image.Header.Value;

      if (header.CompressionMethod != 0)
        throw new NotSupportedException($"PNG: Unknown compression method: {header.CompressionMethod}.");

      if (header.FilterMethod != 0)
        throw new NotSupportedException($"PNG: Unknown filter method: {header.FilterMethod}.");

      PngPipelineManager manager = new PngPipelineManager();

      manager.Pipelines = new List<PngPipeline>();
      manager.Pipelines.Add(new PngCompressionMethod0(header));
      manager.Pipelines.Add(new PngFilterMethod0(header));

      return manager;
    }

    internal byte[] Decode(byte[] buffer)
    {
      for (int i = 0; i < Pipelines.Count; ++i)
      {
        using MemoryStream input = new MemoryStream(buffer);
        buffer = Pipelines[i].Decode(input);
      }

      return buffer;
    }
  }

  internal class PngCompressionMethod0 : PngPipeline
  {
    internal PngCompressionMethod0(PngHeader header)
      : base(header)
    {
    }

    internal override byte[] Decode(Stream input)
    {
      using ZLibStream zlibStream = new ZLibStream(input, CompressionMode.Decompress);
      using MemoryStream output = new MemoryStream();

      zlibStream.CopyTo(output);

      return output.ToArray();
    }
  }

  internal class PngFilterMethod0 : PngPipeline
  {
    internal PngFilterMethod0(PngHeader header)
      : base(header)
    {
    }

    internal override byte[] Decode(Stream input)
    {
      using MemoryStream output = new MemoryStream();
      byte[] scanline = new byte[Header.Pitch];

      for (int i = 0; i < Header.Height; ++i)
      {
        int type = input.ReadByte();
        if (type < 0)
          throw new InvalidDataException("PNG: Unexpected EOF reading a scanline.");

        using ReadOnlySubStream scanlineStream = new ReadOnlySubStream(
          input,
          input.Position,
          Header.Pitch
        );

        if (scanlineStream.Length != scanline.Length)
          throw new InvalidDataException($"PNG: Inconsistent scanline length detected.");

        scanline = DecodeScanline((Types)type, scanlineStream, scanline, Header.SamplesCount);
        output.Write(scanline);
      }

      return output.ToArray();
    }

    private byte[] DecodeScanline(Types type, Stream input, byte[] scanline, int samplesCount)
    {
      byte[] output = new byte[scanline.Length];

      for (int i = 0; i < output.Length; ++i)
      {
        int value = input.ReadByte();
        if (value < 0)
          throw new InvalidDataException("PNG: Unexpected EOF decoding a filter.");

        // NOTE(adm244): 'a','b','c' are corresponding samples of neighboring pixels to 'x'
        //  [+ c + +] [+ b + +]
        //  [+ a + +] [+ x + +]
        byte a = i >= samplesCount ? output[i - samplesCount] : (byte)0;
        byte b = scanline[i];
        byte c = i >= samplesCount ? scanline[i - samplesCount] : (byte)0;
        byte x = (byte)value;

        output[i] = DecodeSample(type, a, b, c, x);
      }

      return output;
    }

    private byte DecodeSample(Types type, byte a, byte b, byte c, byte x)
    {
      switch (type)
      {
        case Types.None:
          return x;
        case Types.Sub:
          return (byte)(x + a);
        case Types.Up:
          return (byte)(x + b);
        case Types.Average:
          return (byte)(x + ((a + b) / 2));
        case Types.Paeth:
          return (byte)(x + Paeth(a, b, c));

        default:
          throw new InvalidDataException($"PNG: Invalid filter type detected: {type}.");
      }
    }

    private byte Paeth(byte a, byte b, byte c)
    {
      int p = a + b - c;
      int pa = Math.Abs(p - a);
      int pb = Math.Abs(p - b);
      int pc = Math.Abs(p - c);

      if (pa <= pb && pa <= pc)
        return a;
      else if (pb <= pc)
        return b;

      return c;
    }

    internal enum Types
    {
      None,
      Sub,
      Up,
      Average,
      Paeth,
    }
  }

  internal class PngImage
  {
    private const string Signature = "\x89PNG\x0d\x0a\x1a\x0a";

    internal PngHeader? Header;
    internal Palette? Palette;
    internal byte[] Buffer;
    internal PixelFormat Format;

    internal Color[] PaletteColors;
    internal MemoryStream ImageDataStream;

    private PngImage(Stream stream)
    {
      Header = null;
      Palette = null;
      Buffer = null;
      Format = default(PixelFormat);

      PaletteColors = null;
      ImageDataStream = new MemoryStream();

      ReadStream(stream);
    }

    private void ReadStream(Stream stream)
    {
      using BinaryReader reader = new BinaryReader(stream);

      string signature = reader.ReadFixedString(Signature.Length, Encoding.Latin1);
      if (signature != Signature)
        throw new InvalidDataException("PNG: Invalid datastream signature encountered.");

      while (!reader.EOF())
      {
        PngChunk chunk = PngChunk.Read(this, reader);
        if (chunk.Type == PngChunk.Types.IEND)
          break;
      }

      PngPipelineManager pipeline = PngPipelineManager.Create(this);
      Buffer = pipeline.Decode(ImageDataStream.ToArray());
      if (Buffer == null || Buffer.Length <= 0)
        throw new InvalidDataException("PNG: Could not decode pixel data.");

      // FIXME(adm244): not sure what to do with output buffer yet;
      // we should deal with dropping support for BMP images first;
      // then it would be clear how we should store image data and
      // where to convert it to an appropriate pixel format

      // Format = Header?.GetPixelFormat();
      Palette = new Palette(PaletteColors, PixelFormat.Rgb24);

      using FileStream fs = new FileStream("/home/egor/ags/image_mine.data", FileMode.Create, FileAccess.Write);
      fs.Write(Buffer);

      // using FileStream fsp = new FileStream("/home/egor/ags/image_mine.pal", FileMode.Create, FileAccess.Write);
      // fsp.Write(Palette?.Entries.ToBuffer(PixelFormat.Argb32));
    }

    internal static Bitmap ReadFile(string filepath)
    {
      using FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);

      PngImage image = new PngImage(stream);
      return new Bitmap(
        (int)image.Header?.Width,
        (int)image.Header?.Height,
        image.Buffer,
        image.Format,
        image.Palette
      );
    }
  }
}
