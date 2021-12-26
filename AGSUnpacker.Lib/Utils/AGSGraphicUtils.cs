using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using AGSUnpacker.Extensions;

namespace AGSUnpacker.Utils
{
  public enum LiteralType
  {
    Symbol,
    Lookback,
  }

  public struct Element
  {
    public LiteralType Type;
    public byte Symbol;
    public long Offset;
    public long Length;

    public void SetSymbol(byte symbol)
    {
      Type = LiteralType.Symbol;
      Symbol = symbol;
    }

    public void SetLookback(long offset, long length)
    {
      Type = LiteralType.Lookback;
      Offset = offset;
      Length = length;
    }
  }

  public enum ColorFormat
  {
    RGB6bits,
    RGB8bits,
    RGBA6bits,
    RGBA8bits,
  }

  public static class ColorFormatExtension
  {
    public static int GetBytesPerPixel(this ColorFormat format)
    {
      switch (format)
      {
        case ColorFormat.RGB6bits:
        case ColorFormat.RGB8bits:
          return 3;

        case ColorFormat.RGBA6bits:
        case ColorFormat.RGBA8bits:
          return 4;

        default:
          throw new NotImplementedException("Unimplemented palette format!");
      }
    }
  }

  public static class AGSGraphicUtils
  {
    public static int ToABGR(Color color)
    {
      return (color.A << 24) | (color.B << 16) | (color.G << 8) | color.R;
    }

    public static Color FromABGR(int abgr)
    {
      int red   = ((abgr >>  0) & 0xFF);
      int green = ((abgr >>  8) & 0xFF);
      int blue  = ((abgr >> 16) & 0xFF);
      int alpha = ((abgr >> 24) & 0xFF);

      return Color.FromArgb(alpha, red, green, blue);
    }

    public static PixelFormat ReadBitmapPixelFormat(string filepath)
    {
      PixelFormat format = PixelFormat.Undefined;

      using (FileStream file = new FileStream(filepath, FileMode.Open))
      {
        //NOTE(adm244): is there any reason for us to use ASCII encoding here specifically?
        using (BinaryReader reader = new BinaryReader(file, Encoding.ASCII))
        {
          reader.BaseStream.Seek(28, SeekOrigin.Begin);
          UInt16 bitCount = reader.ReadUInt16();

          format = BitmapExtension.GetPixelFormat(bitCount / 8);
        }
      }

      return format;
    }

    public static Bitmap ConvertToBitmap(byte[] buffer, int width, int height, int bytesPerPixel, Color[] palette)
    {
      PixelFormat format = BitmapExtension.GetPixelFormat(bytesPerPixel);
      Bitmap bitmap = new Bitmap(width, height, format);
      bitmap.SetPixels(buffer);

      if (bytesPerPixel == 1)
        bitmap.SetPalette(palette);

      return bitmap;
    }

    public static byte[] ColorsToBytes(Color[] colors, ColorFormat format)
    {
      switch (format)
      {
        case ColorFormat.RGB6bits:
        case ColorFormat.RGB8bits:
          return ColorsToRGB(colors, format);

        case ColorFormat.RGBA6bits:
        case ColorFormat.RGBA8bits:
          return ColorsToRGBA(colors, format);

        default:
          throw new NotImplementedException("Unimplemented palette format!");
      }
    }

    private static byte[] ColorsToRGB(Color[] colors, ColorFormat format)
    {
      int bytesPerPixel = format.GetBytesPerPixel();
      if (bytesPerPixel != 3)
        throw new InvalidDataException("Invalid color format for RGB!");

      byte[] buffer = new byte[colors.Length * bytesPerPixel];

      for (int i = 0; i < colors.Length; ++i)
      {
        int red   = colors[i].R;
        int green = colors[i].G;
        int blue  = colors[i].B;

        if (format == ColorFormat.RGB6bits)
        {
          //TODO(adm244): consider moving this into MathUtils or something
          red   = (int)((red   / 256f) * 64f);
          green = (int)((green / 256f) * 64f);
          blue  = (int)((blue  / 256f) * 64f);
        }

        buffer[bytesPerPixel * i + 0] = (byte)red;
        buffer[bytesPerPixel * i + 1] = (byte)green;
        buffer[bytesPerPixel * i + 2] = (byte)blue;
      }

      return buffer;
    }

    private static byte[] ColorsToRGBA(Color[] colors, ColorFormat format)
    {
      int bytesPerPixel = format.GetBytesPerPixel();
      if (bytesPerPixel != 4)
        throw new InvalidDataException("Invalid color format for RGBA!");

      byte[] buffer = new byte[colors.Length * bytesPerPixel];

      for (int i = 0; i < colors.Length; ++i)
      {
        int red   = colors[i].R;
        int green = colors[i].G;
        int blue  = colors[i].B;
        int alpha = colors[i].A;

        if (format == ColorFormat.RGBA6bits)
        {
          red   = (int)((red   / 256f) * 64f);
          green = (int)((green / 256f) * 64f);
          blue  = (int)((blue  / 256f) * 64f);
          alpha = (int)((alpha / 256f) * 64f);
        }

        buffer[bytesPerPixel * i + 0] = (byte)red;
        buffer[bytesPerPixel * i + 1] = (byte)green;
        buffer[bytesPerPixel * i + 2] = (byte)blue;
        buffer[bytesPerPixel * i + 3] = (byte)alpha;
      }

      return buffer;
    }

    public static Color[] BytesToColors(byte[] buffer, ColorFormat format)
    {
      switch (format)
      {
        case ColorFormat.RGB6bits:
        case ColorFormat.RGB8bits:
          return RGBToColors(buffer, format);

        case ColorFormat.RGBA6bits:
        case ColorFormat.RGBA8bits:
          return RGBAToColors(buffer, format);

        default:
          throw new NotImplementedException("Unimplemented palette format!");
      }
    }

    private static Color[] RGBToColors(byte[] buffer, ColorFormat format)
    {
      int bytesPerPixel = format.GetBytesPerPixel();
      if (bytesPerPixel != 3)
        throw new InvalidDataException("Invalid color format for RGB!");

      int count = (buffer.Length / bytesPerPixel);
      Color[] colors = new Color[count];

      for (int i = 0; i < colors.Length; ++i)
      {
        int red   = buffer[bytesPerPixel * i + 0];
        int green = buffer[bytesPerPixel * i + 1];
        int blue  = buffer[bytesPerPixel * i + 2];

        if (format == ColorFormat.RGB6bits)
        {
          blue  = (byte)((blue  / 64f) * 256f);
          green = (byte)((green / 64f) * 256f);
          red   = (byte)((red   / 64f) * 256f);
        }

        colors[i] = Color.FromArgb(red, green, blue);
      }

      return colors;
    }

    private static Color[] RGBAToColors(byte[] buffer, ColorFormat format)
    {
      int bytesPerPixel = format.GetBytesPerPixel();
      if (bytesPerPixel != 4)
        throw new InvalidDataException("Invalid color format for RGBA!");

      int count = (buffer.Length / bytesPerPixel);
      Color[] colors = new Color[count];

      for (int i = 0; i < colors.Length; ++i)
      {
        int red   = buffer[bytesPerPixel * i + 0];
        int green = buffer[bytesPerPixel * i + 1];
        int blue  = buffer[bytesPerPixel * i + 2];
        int alpha = buffer[bytesPerPixel * i + 3];

        if (format == ColorFormat.RGBA6bits)
        {
          red   = (int)((red   / 64f) * 256f);
          green = (int)((green / 64f) * 256f);
          blue  = (int)((blue  / 64f) * 256f);
          alpha = (int)((alpha / 64f) * 256f);
        }

        colors[i] = Color.FromArgb(alpha, red, green, blue);
      }

      return colors;
    }

    public static Color[] ReadPalette(BinaryReader reader, ColorFormat format)
    {
      //TODO(adm244): investigate if a palette can have a different colors count in it
      int size = (256 * format.GetBytesPerPixel());
      byte[] buffer = reader.ReadBytes(size);

      return AGSGraphicUtils.BytesToColors(buffer, format);
    }

    public static void WritePalette(BinaryWriter writer, Color[] palette, ColorFormat format)
    {
      byte[] buffer = ColorsToBytes(palette, format);
      writer.Write((byte[])buffer);
    }

    public static Bitmap ReadAllegroImage(BinaryReader reader)
    {
      Int16 bytesPerPixel = 1;
      Int16 width = reader.ReadInt16();
      Int16 height = reader.ReadInt16();

      byte[] buffer = ReadAllegro(reader, width, height);
      Debug.Assert(buffer.Length == (width * height * bytesPerPixel));

      Color[] palette = ReadPalette(reader, ColorFormat.RGB6bits);
      return ConvertToBitmap(buffer, width, height, bytesPerPixel, palette);
    }

    public static void WriteAllegroImage(BinaryWriter writer, Bitmap bitmap)
    {
      writer.Write((Int16)bitmap.Width);
      writer.Write((Int16)bitmap.Height);

      byte[] pixels = bitmap.GetPixels();
      WriteAllegro(writer, pixels, bitmap.Width, bitmap.Height);

      byte[] palette = ColorsToBytes(bitmap.Palette.Entries, ColorFormat.RGB6bits);
      writer.Write((byte[])palette);
    }

    public static Bitmap ReadLZ77Image(BinaryReader reader, int bytesPerPixel)
    {
      byte[] bufferPalette = reader.ReadBytes(256 * sizeof(UInt32));
      Int32 sizeUncompressed = reader.ReadInt32();
      Int32 sizeCompressed = reader.ReadInt32();

      int width;
      int height;
      byte[] bufferPixels = ReadLZ77(reader, sizeUncompressed, bytesPerPixel, out width, out height);

      ColorFormat paletteFormat = ColorFormat.RGBA8bits;
      //NOTE(adm244): AGS is using only 6-bits per channel for an indexed image,
      // so we have to convert it to full 8-bit range
      if (bytesPerPixel == 1)
        paletteFormat = ColorFormat.RGBA6bits;

      Color[] palette = BytesToColors(bufferPalette, paletteFormat);
      Bitmap bitmap = ConvertToBitmap(bufferPixels, width, height, bytesPerPixel, palette);

      //NOTE(adm244): since AGS doesn't use alpha-channel for room backgrounds it is nullyfied
      // and we have to get rid of it by converting image into 24-bit pixel format (no alpha channel)
      bitmap = bitmap.Convert(PixelFormat.Format24bppRgb);

      return bitmap;
    }

    public static void WriteLZ77Image(BinaryWriter writer, Bitmap bitmap, int bytesPerPixel)
    {
      //FIX(adm244): 24-bit bitmaps are all messed up !!!
      //TODO(adm244): check if that's still true or I just forgot to remove this scary FIX

      byte[] palette = ColorsToBytes(bitmap.Palette.Entries, ColorFormat.RGBA6bits);
      writer.Write((byte[])palette);

      //NOTE(adm244): convert bitmap to match requested bytesPerPixel
      if (bitmap.GetBytesPerPixel() != bytesPerPixel)
        bitmap.Convert(BitmapExtension.GetPixelFormat(bytesPerPixel));

      byte[] pixels = bitmap.GetPixels();
      Debug.Assert(pixels.Length == (bitmap.Width * bitmap.Height * bytesPerPixel));

      byte[] buffer = AppendImageSize(bitmap.Width, bitmap.Height, bytesPerPixel, pixels);
      byte[] bufferCompressed = LZ77Compress(buffer);

      writer.Write((UInt32)buffer.Length);
      writer.Write((UInt32)bufferCompressed.Length);
      writer.Write((byte[])bufferCompressed);
    }

    public static byte[] WriteRLE8Rows(byte[] buffer, int width, int height)
    {
      using (MemoryStream memoryStream = new MemoryStream(buffer.Length))
      {
        for (int y = 0; y < height; ++y)
        {
          byte[] row = new byte[width];
          Buffer.BlockCopy(buffer, y * width, row, 0, width);

          byte[] bufferCompressed = AGSGraphicUtils.WriteRLEData8(row);
          memoryStream.Write(bufferCompressed, 0, bufferCompressed.Length);
        }

        return memoryStream.ToArray();
      }
    }

    public static byte[] WriteRLE16Rows(byte[] buffer, int width, int height)
    {
      using (MemoryStream memoryStream = new MemoryStream(buffer.Length))
      {
        for (int y = 0; y < height; ++y)
        {
          UInt16[] row = new UInt16[width];
          Buffer.BlockCopy(buffer, y * width * 2, row, 0, width * 2);

          byte[] bufferCompressed = AGSGraphicUtils.WriteRLEData16(row);
          memoryStream.Write(bufferCompressed, 0, bufferCompressed.Length);
        }

        return memoryStream.ToArray();
      }
    }

    public static byte[] WriteRLE32Rows(byte[] buffer, int width, int height)
    {
      using (MemoryStream memoryStream = new MemoryStream(buffer.Length))
      {
        for (int y = 0; y < height; ++y)
        {
          UInt32[] row = new UInt32[width];
          Buffer.BlockCopy(buffer, y * width * 4, row, 0, width * 4);

          byte[] bufferCompressed = AGSGraphicUtils.WriteRLEData32(row);
          memoryStream.Write(bufferCompressed, 0, bufferCompressed.Length);
        }

        return memoryStream.ToArray();
      }
    }

    // compression related stuff below
    //TODO(adm244): move compression related methods into AGSCompressionUtils

    public static byte[] ReadRLE8(BinaryReader reader, long sizeCompressed, long sizeUncompressed)
    {
      byte[] buffer = new byte[sizeUncompressed];
      int positionImage = 0;

      long positionBase = reader.BaseStream.Position;
      for (long n = 0; n < sizeCompressed; n = (reader.BaseStream.Position - positionBase))
      {
        sbyte control = (sbyte)reader.ReadByte();
        if (control == -128)
          control = 0;

        if (control < 0)
        {
          //NOTE(adm244): literal run
          int runCount = (1 - control);
          byte value = reader.ReadByte();
          for (int j = 0; j < runCount; ++j)
          {
            buffer[positionImage] = value;
            ++positionImage;
          }
        }
        else
        {
          //NOTE(adm244): literal sequence
          int literalsCount = (control + 1);
          for (int j = 0; j < literalsCount; ++j)
          {
            buffer[positionImage] = reader.ReadByte();
            ++positionImage;
          }
        }
      }

      return buffer;
    }

    public static byte[] ReadRLE16(BinaryReader reader, long sizeCompressed, long sizeUncompressed)
    {
      byte[] buffer = new byte[sizeUncompressed];
      int positionImage = 0;

      long positionBase = reader.BaseStream.Position;
      for (long n = 0; n < sizeCompressed; n = (reader.BaseStream.Position - positionBase))
      {
        sbyte control = (sbyte)reader.ReadByte();
        if (control == -128)
          control = 0;

        if (control < 0)
        {
          //NOTE(adm244): literal run
          int runCount = (1 - control);
          UInt16 value = reader.ReadUInt16();
          for (int j = 0; j < runCount; ++j)
          {
            buffer[positionImage + 0] = (byte)(value >> 0);
            buffer[positionImage + 1] = (byte)(value >> 8);
            positionImage += 2;
          }
        }
        else
        {
          //NOTE(adm244): literal sequence
          int literalsCount = (control + 1);
          for (int j = 0; j < literalsCount; ++j)
          {
            UInt16 value = reader.ReadUInt16();
            buffer[positionImage + 0] = (byte)(value >> 0);
            buffer[positionImage + 1] = (byte)(value >> 8);
            positionImage += 2;
          }
        }
      }

      return buffer;
    }

    public static byte[] ReadRLE32(BinaryReader reader, long sizeCompressed, long sizeUncompressed)
    {
      byte[] buffer = new byte[sizeUncompressed];
      int positionImage = 0;

      long positionBase = reader.BaseStream.Position;
      for (long n = 0; n < sizeCompressed; n = (reader.BaseStream.Position - positionBase))
      {
        sbyte control = (sbyte)reader.ReadByte();
        if (control == -128)
          control = 0;

        if (control < 0)
        {
          //NOTE(adm244): literal run
          int runCount = (1 - control);
          UInt32 value = reader.ReadUInt32();
          for (int j = 0; j < runCount; ++j)
          {
            buffer[positionImage + 0] = (byte)(value >> 0);
            buffer[positionImage + 1] = (byte)(value >> 8);
            buffer[positionImage + 2] = (byte)(value >> 16);
            buffer[positionImage + 3] = (byte)(value >> 24);
            positionImage += 4;
          }
        }
        else
        {
          //NOTE(adm244): literal sequence
          int literalsCount = (control + 1);
          for (int j = 0; j < literalsCount; ++j)
          {
            UInt32 value = reader.ReadUInt32();
            buffer[positionImage + 0] = (byte)(value >> 0);
            buffer[positionImage + 1] = (byte)(value >> 8);
            buffer[positionImage + 2] = (byte)(value >> 16);
            buffer[positionImage + 3] = (byte)(value >> 24);
            positionImage += 4;
          }
        }
      }

      return buffer;
    }

    public static byte[] WriteRLEData8(byte[] buffer)
    {
      const int maxRuns = 128;
      byte[] bufferCompressed = new byte[maxRuns];
      int bufferPosition = 0;

      using (MemoryStream stream = new MemoryStream())
      {
        for (long i = 0; i < buffer.Length; )
        {
          byte value = buffer[i];

          int blockLength = (maxRuns - 1);
          if (blockLength > (buffer.Length - i))
            blockLength = (int)(buffer.Length - i);

          int run = 1;
          while ((run < blockLength) && (buffer[i + run] == value))
            ++run;

          if ((run > 1) || (bufferPosition == maxRuns) || (i == (buffer.Length - 1)))
          {
            //NOTE(adm244): encode a run
            if (bufferPosition > 0)
            {
              stream.WriteByte((byte)(bufferPosition - 1));
              stream.Write(bufferCompressed, 0, bufferPosition);
              bufferPosition = 0;
            }

            stream.WriteByte((byte)(1 - run));
            stream.WriteByte((byte)(value));
            i += run;
          }
          else
          {
            //NOTE(adm244): encode a sequence
            bufferCompressed[bufferPosition] = value;
            ++bufferPosition;
            ++i;
          }
        }

        return stream.ToArray();
      }
    }

    public static byte[] WriteRLEData16(UInt16[] buffer)
    {
      const int maxRuns = 128;
      UInt16[] bufferCompressed = new UInt16[maxRuns];
      int bufferPosition = 0;

      using (MemoryStream stream = new MemoryStream())
      {
        for (long i = 0; i < buffer.Length; )
        {
          UInt16 value = buffer[i];

          int blockLength = (maxRuns - 1);
          if (blockLength > (buffer.Length - i))
            blockLength = (int)(buffer.Length - i);

          int run = 1;
          while ((run < blockLength) && (buffer[i + run] == value))
            ++run;

          if ((run > 1) || (bufferPosition == maxRuns) || (i == (buffer.Length - 1)))
          {
            //NOTE(adm244): encode a run
            if (bufferPosition > 0)
            {
              stream.WriteByte((byte)(bufferPosition - 1));
              for (int j = 0; j < bufferPosition; ++j)
              {
                stream.WriteByte((byte)(bufferCompressed[j]));
                stream.WriteByte((byte)(bufferCompressed[j] >> 8));
              }
              bufferPosition = 0;
            }

            stream.WriteByte((byte)(1 - run));
            stream.WriteByte((byte)(value));
            stream.WriteByte((byte)(value >> 8));
            i += run;
          }
          else
          {
            //NOTE(adm244): encode a sequence
            bufferCompressed[bufferPosition] = value;
            ++bufferPosition;
            ++i;
          }
        }

        return stream.ToArray();
      }
    }

    public static byte[] WriteRLEData32(UInt32[] buffer)
    {
      const int maxRuns = 128;
      UInt32[] bufferCompressed = new UInt32[maxRuns];
      int bufferPosition = 0;

      using (MemoryStream stream = new MemoryStream())
      {
        for (long i = 0; i < buffer.Length; )
        {
          UInt32 value = buffer[i];

          int blockLength = (maxRuns - 1);
          if (blockLength > (buffer.Length - i))
            blockLength = (int)(buffer.Length - i);

          int run = 1;
          while ((run < blockLength) && (buffer[i + run] == value))
            ++run;

          if ((run > 1) || (bufferPosition == maxRuns) || (i == (buffer.Length - 1)))
          {
            //NOTE(adm244): encode a run
            if (bufferPosition > 0)
            {
              stream.WriteByte((byte)(bufferPosition - 1));
              for (int j = 0; j < bufferPosition; ++j)
              {
                stream.WriteByte((byte)(bufferCompressed[j]));
                stream.WriteByte((byte)(bufferCompressed[j] >> 8));
                stream.WriteByte((byte)(bufferCompressed[j] >> 16));
                stream.WriteByte((byte)(bufferCompressed[j] >> 24));
              }
              bufferPosition = 0;
            }

            stream.WriteByte((byte)(1 - run));
            stream.WriteByte((byte)(value));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
            i += run;
          }
          else
          {
            //NOTE(adm244): encode a sequence
            bufferCompressed[bufferPosition] = value;
            ++bufferPosition;
            ++i;
          }
        }

        return stream.ToArray();
      }
    }

    private static byte[] ReadAllegro(BinaryReader reader, int width, int height)
    {
      using (MemoryStream stream = new MemoryStream())
      {
        for (int y = 0; y < height; ++y)
        {
          int pixelsRead = 0;
          while (pixelsRead < width)
          {
            sbyte index = (sbyte)reader.ReadByte();
            if (index == -128) index = 0;

            if (index < 0)
            {
              int count = (1 - index);
              byte value = reader.ReadByte();

              while ((count--) > 0)
                stream.WriteByte(value);

              pixelsRead += (1 - index);
            }
            else
            {
              byte[] buffer = reader.ReadBytes(index + 1);
              stream.Write(buffer, 0, buffer.Length);
              pixelsRead += (index + 1);
            }
          }
        }

        return stream.ToArray();
      }
    }

    private static void WriteAllegro(BinaryWriter writer, byte[] buffer, int width, int height)
    {
      for (int y = 0; y < height; ++y)
      {
        byte[] row = new byte[width];
        Buffer.BlockCopy(buffer, y * width, row, 0, width);

        byte[] bufferCompressed = WriteRLEData8(row);
        writer.Write(bufferCompressed, 0, bufferCompressed.Length);
      }
    }

    private static byte[] ReadLZ77(BinaryReader reader, long sizeUncompressed, int bytesPerPixel, out int width, out int height)
    {
      /*
       * AGS background image decompression algorithm:
       * 
       * byte[] output = new byte[uncompressed_size];
       * int output_pos = 0;
       * 
       * do {
       *   byte control = stream.read8();
       *   for (int mask = 1; mask & 0xFF; mask <<= 1) {
       *     if (control & mask) {
       *       uint16 runlength = stream.read16();
       *       
       *       //NOTE(adm244): first 12 bits are used to encode offset from current position in stream
       *       // which is equals to length of the buffer that was used to store a dictionary - 1 (0x0FFF = 4096 - 1)
       *       uint16 sequence_start_offset = runlength & 0x0FFF;
       *       
       *       //NOTE(adm244): +3 part is here to shift [min,max] range by 3 (resulting in [3, 18]),
       *       // because there's no point in compressing sequences of bytes with length 1 or 2.
       *       // If sequence length is 1 then you just output it, no compression can be applied.
       *       // If sequence length is 2 then it doesn't matter if you compress it or not,
       *       // because 2 bytes are already used to store offset and length of that sequence,
       *       // so the result will have the same length.
       *       byte sequence_length = (runlength >> 12) + 3;
       *       
       *       //NOTE(adm244): -1 because current position in the stream points to the next byte
       *       uin16 sequence_pos = output_pos - sequence_start_offset - 1;
       *       for (int i = 0; i < sequence_length; ++i) {
       *         output[output_pos] = output[sequence_pos];
       *         output_pos++;
       *         sequence_pos++;
       *       }
       *     } else {
       *       output[output_pos] = stream.read8();
       *       output_pos++;
       *     }
       *     
       *     if (output_pos >= uncompressed_size)
       *       break;
       *   }
       * } while (!stream.EOF());
       * 
       */

      byte[] output = new byte[sizeUncompressed];
      long output_pos = 0;

      while (output_pos < sizeUncompressed)
      {
        byte control = reader.ReadByte();
        for (int mask = 1; (mask & 0xFF) != 0; mask <<= 1)
        {
          if ((control & mask) == 0)
          {
            output[output_pos] = reader.ReadByte();
            output_pos++;
          }
          else
          {
            UInt16 runlength = reader.ReadUInt16();
            UInt16 sequence_start_offset = (UInt16)(runlength & 0x0FFF);
            byte sequence_length = (byte)((runlength >> 12) + 3);

            long sequence_pos = output_pos - sequence_start_offset - 1;
            for (int i = 0; i < sequence_length; ++i)
            {
              output[output_pos] = output[sequence_pos];
              output_pos++;
              sequence_pos++;
            }
          }

          if (output_pos >= sizeUncompressed)
            break;
        }
      }

      //TODO(adm244): get from DTA info
      //TODO(adm244): try to remember what this TODO means...
      //TODO(adm244): consider using a utils function to convert from a byte buffer to int32
      width = ((output[3] << 24) | (output[2] << 16) | (output[1] << 8) | output[0]) / bytesPerPixel;
      height = ((output[7] << 24) | (output[6] << 16) | (output[5] << 8) | output[4]);

      byte[] pixels = new byte[output.Length - 8];
      Array.Copy(output, 8, pixels, 0, pixels.Length);

      return pixels;
    }

    //TODO(adm244): this method looks suspicious, investigate
    private static byte[] AppendImageSize(int width, int height, int bytesPerPixel, byte[] pixels)
    {
      byte[] rawData = new byte[pixels.Length + 8];

      int newWidth = width * bytesPerPixel;
      rawData[0] = (byte)(newWidth);
      rawData[1] = (byte)(newWidth >> 8);
      rawData[2] = (byte)(newWidth >> 16);
      rawData[3] = (byte)(newWidth >> 24);

      rawData[4] = (byte)(height);
      rawData[5] = (byte)(height >> 8);
      rawData[6] = (byte)(height >> 16);
      rawData[7] = (byte)(height >> 24);

      //rawData.SetValue(width * bpp, 0);

      Array.Copy(pixels, 0, rawData, 8, pixels.Length);

      return rawData;
    }

    //NOTE(adm244): it performs worse than the original (i.e. low speed) but
    // the file size is actually smaller since we don't interrupt the sequence
    private static byte[] LZ77Compress(byte[] buffer)
    {
      using (MemoryStream stream = new MemoryStream(buffer.Length))
      {
        const long LookbackSize = 4095;
        const byte LookbackSequenceLength = 18;
        const byte ElementsSize = 8;

        Element[] elements = new Element[ElementsSize];
        byte elementsCount = 0;

        long bufferPosition = 0;
        while (bufferPosition < buffer.Length)
        {
          //NOTE(adm244): PART 1: Find largest matching sequence in a lookback buffer

          //NOTE(adm244): BRUTE-FORCE approach

          //NOTE(adm244): lookback buffer includes current buffer position
          long lookbackLength = Math.Min(bufferPosition, LookbackSize);
          long lookbackStart = bufferPosition - lookbackLength;

          long bestRun = 0;
          long bestRunOffset = 0;
          long lookbackEnd = lookbackStart + lookbackLength;
          for (long lookbackPosition = lookbackStart; lookbackPosition < lookbackEnd; )
          {
            //NOTE(adm244): allow lookback buffer to go beyond current buffer position
            long sequenceLength = 0;
            long lookbackSubEnd = (lookbackPosition + LookbackSequenceLength);
            for (long lookbackSubPosition = lookbackPosition; lookbackSubPosition < lookbackSubEnd; ++lookbackSubPosition)
            {
              long bufferSubPosition = bufferPosition + (lookbackSubPosition - lookbackPosition);
              if (bufferSubPosition == buffer.Length)
                break;

              if (buffer[lookbackSubPosition] == buffer[bufferSubPosition])
                ++sequenceLength;
              else
                break;
            }

            if (bestRun < sequenceLength)
            {
              bestRun = sequenceLength;
              bestRunOffset = bufferPosition - lookbackPosition - 1;
            }

            if (sequenceLength > 0)
              lookbackPosition += sequenceLength;
            else
              lookbackPosition += 1;
          }

          //NOTE(adm244): PART 2: Encode either LOOKBACK (RUN) or LITERAL

          if (bestRun >= 3)
          {
            //NOTE(adm244): encode looback (run)
            elements[elementsCount].SetLookback(bestRunOffset, bestRun);
            elementsCount += 1;

            bufferPosition += bestRun;
          }
          else
          {
            //NOTE(adm244): encode literal
            elements[elementsCount].SetSymbol(buffer[bufferPosition]);
            elementsCount += 1;

            bufferPosition += 1;
          }

          //NOTE(adm244): PART 3: Flush elements buffer if it's full or we're at the end of a stream

          if ((elementsCount == ElementsSize) || (bufferPosition == buffer.Length))
          {
            int controlMask = 0;
            int elementsLength = (elementsCount < ElementsSize) ? elementsCount : ElementsSize;

            //NOTE(adm244): make mask for control byte
            for (int i = 0; i < elementsLength; ++i)
            {
              if (elements[i].Type == LiteralType.Lookback)
                controlMask |= (1 << i);
            }

            Debug.Assert(controlMask == (int)((byte)(controlMask)));
            stream.WriteByte((byte)controlMask);

            //NOTE(adm244): write elements
            for (int i = 0; i < elementsLength; ++i)
            {
              if (elements[i].Type == LiteralType.Lookback)
              {
                UInt16 offset = (UInt16)(elements[i].Offset & 0x0FFF);
                byte length = (byte)(elements[i].Length - 3);

                Debug.Assert(elements[i].Offset == (long)(offset));
                Debug.Assert(elements[i].Length == (long)((length + 3)));

                Debug.Assert((length >= 0) && (length <= 15));

                UInt16 runlength = (UInt16)(offset | (length << 12));
                stream.WriteByte((byte)((runlength >> 0) & 0xFF));
                stream.WriteByte((byte)((runlength >> 8) & 0xFF));
              }
              else
              {
                stream.WriteByte((byte)elements[i].Symbol);
              }
            }

            elementsCount = 0;
          }
        }

        return stream.ToArray();
      }
    }
  }
}
