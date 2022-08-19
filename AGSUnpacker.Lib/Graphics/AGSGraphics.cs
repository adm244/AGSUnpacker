using System;
using System.Diagnostics;
using System.IO;

using AGSUnpacker.Graphics;
using AGSUnpacker.Graphics.Formats;
using AGSUnpacker.Lib.Utils;

namespace AGSUnpacker.Lib.Graphics
{
  public static class AGSGraphics
  {
    // NOTE(adm244): assumes there's at least one bright color in a palette
    private static PixelFormat GetPaletteFormat(byte[] buffer)
    {
      for (int i = 0; i < buffer.Length; ++i)
      {
        if (buffer[i] >= 64)
          return PixelFormat.Rgb24;
      }

      return PixelFormat.Rgb666;
    }

    public static Palette ReadPalette(BinaryReader reader)
    {
      int size = 256 * 3;
      byte[] buffer = reader.ReadBytes(size);

      // NOTE(adm244): 2.72 (and newer?) stores palette in rgb888 format
      // and there's no way to know for sure which format is used unless we know engine version
      // which we don't, it's not stored in sprite set file
      PixelFormat format = GetPaletteFormat(buffer);

      return Palette.FromBuffer(buffer, format);
    }

    public static Palette ReadPalette(BinaryReader reader, PixelFormat format)
    {
      int size = 256 * 3;
      byte[] buffer = reader.ReadBytes(size);
      return Palette.FromBuffer(buffer, format);
    }

    public static void WritePalette(BinaryWriter writer, Palette palette)
    {
      if (!palette.SourceFormat.HasValue)
        throw new ArgumentNullException(nameof(palette.SourceFormat));

      WritePalette(writer, palette, palette.SourceFormat.Value);
    }

    public static void WritePalette(BinaryWriter writer, Palette palette, PixelFormat format)
    {
      byte[] buffer = palette.ToBuffer(format);
      writer.Write((byte[])buffer);
    }

    public static Bitmap ReadAllegroImage(BinaryReader reader)
    {
      Int16 bytesPerPixel = 1;
      Int16 width = reader.ReadInt16();
      Int16 height = reader.ReadInt16();

      // NOTE(adm244): since commit 9a6b55bfe78bf0e9f24b2e3c2d1b073f850fae50
      // "Common: simplified savecompressed_allegro() and renamed for clarity <...>"
      // they "simplified" out the fact that image was compressed line-by-line
      // and now it's compressed as a single buffer. Luckilly for them, it didn't break
      // backwards-compatibility due to decompressors lack of care whether input
      // was chuncked or not. (3.6.0.13)
      // 
      // This means we now can't rely on decompression method that expects input
      // buffer to be compressed as line-by-line and forced to use a generic version
      // which what "AGSCompression.ReadRLE8()" is doing...
      //byte[] buffer = AGSCompression.ReadAllegro(reader, width, height);
      byte[] buffer = AGSCompression.ReadRLE8(reader, width * height * bytesPerPixel);
      Debug.Assert(buffer.Length == (width * height * bytesPerPixel));

      PixelFormat format = PixelFormatExtension.FromBytesPerPixel(bytesPerPixel);

      // NOTE(adm244): in this case palette is always(?) rgb666
      Palette palette = ReadPalette(reader, PixelFormat.Rgb666);

      return new Bitmap(width, height, buffer, format, palette);
    }

    public static void WriteAllegroImage(BinaryWriter writer, Bitmap image)
    {
      writer.Write((Int16)image.Width);
      writer.Write((Int16)image.Height);

      //byte[] pixels = image.InternalImage.GetPixels();
      byte[] pixels = image.GetPixels();

      // NOTE(adm244): despite the fact described in "ReadAllegroImage",
      // we still compress image on line-by-line basis to support pre 3.6.0.13 engine versions
      AGSCompression.WriteAllegro(writer, pixels, image.Width, image.Height);

      if (image.Palette == null)
        throw new ArgumentException();

      // NOTE(adm244): in this case palette is always(?) rgb666
      byte[] palette = image.Palette?.ToBuffer(PixelFormat.Rgb666);
      writer.Write((byte[])palette);
    }

    public static Bitmap ReadLZ77Image(BinaryReader reader, int bytesPerPixel)
    {
      byte[] bufferPalette = reader.ReadBytes(256 * sizeof(UInt32));
      Int32 sizeUncompressed = reader.ReadInt32();

      // TODO(adm244): check compressed size; or read buffer first and then decode
      Int32 sizeCompressed = reader.ReadInt32();

      byte[] bufferPixels = ReadLZ77Image(reader, sizeUncompressed, bytesPerPixel, out int width, out int height);
      PixelFormat format = PixelFormatExtension.FromBytesPerPixel(bytesPerPixel);

      // CHECK(adm244): palette format in room files
      //PixelFormat paletteFormat = PixelFormat.Argb32;
      //if (format == PixelFormat.Indexed)
      PixelFormat paletteFormat = PixelFormat.Argb6666;

      Palette palette = Palette.FromBuffer(bufferPalette, paletteFormat);

      if (format == PixelFormat.Indexed)
        return new Bitmap(width, height, bufferPixels, format, palette);

      Bitmap bitmap = new Bitmap(width, height, bufferPixels, format);
      
      // NOTE(adm244): removes null-alpha; see AGSGraphics.ReadSprite
      if (bitmap.Format == PixelFormat.Argb32)
        bitmap = bitmap.Convert(PixelFormat.Rgb24);

      return bitmap;
    }

    private static byte[] ReadLZ77Image(BinaryReader reader, long sizeUncompressed, int bytesPerPixel, out int width, out int height)
    {
      byte[] buffer = AGSCompression.ReadLZ77(reader, sizeUncompressed);

      width = BitConverter.ToInt32(buffer, 0) / bytesPerPixel;
      height = BitConverter.ToInt32(buffer, 4);

      //TODO(adm244): consider using a utils function to convert from a byte buffer to int32
      //width = ((buffer[3] << 24) | (buffer[2] << 16) | (buffer[1] << 8) | buffer[0]) / bytesPerPixel;
      //height = ((buffer[7] << 24) | (buffer[6] << 16) | (buffer[5] << 8) | buffer[4]);

      // TODO(adm244): consider switching to Spans
      byte[] pixels = new byte[buffer.Length - 8];
      Array.Copy(buffer, 8, pixels, 0, pixels.Length);

      return pixels;
    }

    public static void WriteLZ77Image(BinaryWriter writer, Bitmap image, int bytesPerPixel)
    {
      // CHECK(adm244): palette format in room files
      //PixelFormat paletteFormat = PixelFormat.Argb32;
      // CHECK(adm244): 2.54 crashes if palette is not 6-bits in some cases (if some >0x3F values are present)
      // maybe we don't care for 8-bits palette then? double check that
      //if (image.Format == PixelFormat.Indexed)
      PixelFormat paletteFormat = PixelFormat.Argb6666;

      Palette palette = image.Palette.HasValue ? image.Palette.Value : AGSSpriteSet.DefaultPalette;
      byte[] paletteBuffer = palette.ToBuffer(paletteFormat);
      writer.Write((byte[])paletteBuffer);

      //NOTE(adm244): convert bitmap to match requested bytesPerPixel
      PixelFormat targetFormat = PixelFormatExtension.FromBytesPerPixel(bytesPerPixel);
      if (image.Format != targetFormat)
        image = image.Convert(targetFormat, discardAlpha: true);

      byte[] pixels = image.GetPixels();
      Debug.Assert(pixels.Length == (image.Width * image.Height * bytesPerPixel));

      // TODO(adm244): use stream write to directly write size and pixel data
      byte[] buffer = PreAppendImageSize(image.Width, image.Height, bytesPerPixel, pixels);
      byte[] bufferCompressed = AGSCompression.WriteLZ77(buffer);

      writer.Write((UInt32)buffer.Length);
      writer.Write((UInt32)bufferCompressed.Length);
      writer.Write((byte[])bufferCompressed);
    }

    // TODO(adm244): remove this method
    private static byte[] PreAppendImageSize(int width, int height, int bytesPerPixel, byte[] pixels)
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
  }
}
