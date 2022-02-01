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
    public static Palette ReadPalette(BinaryReader reader, PixelFormat format)
    {
      //TODO(adm244): investigate if a palette can have a different colors count in it
      int size = (256 * format.GetBytesPerPixel());
      byte[] buffer = reader.ReadBytes(size);
      return Palette.FromBuffer(buffer, format);
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

      byte[] buffer = AGSCompression.ReadAllegro(reader, width, height);
      Debug.Assert(buffer.Length == (width * height * bytesPerPixel));

      PixelFormat format = PixelFormatExtension.FromBytesPerPixel(bytesPerPixel);
      Palette palette = ReadPalette(reader, PixelFormat.Rgb666);
      return new Bitmap(width, height, buffer, format, palette);
    }

    public static void WriteAllegroImage(BinaryWriter writer, Bitmap image)
    {
      writer.Write((Int16)image.Width);
      writer.Write((Int16)image.Height);

      //byte[] pixels = image.InternalImage.GetPixels();
      byte[] pixels = image.GetPixels();
      AGSCompression.WriteAllegro(writer, pixels, image.Width, image.Height);

      if (image.Palette == null)
        throw new ArgumentException();

      byte[] palette = image.Palette?.ToBuffer(PixelFormat.Rgb666);
      writer.Write((byte[])palette);
    }

    public static Bitmap ReadLZ77Image(BinaryReader reader, int bytesPerPixel)
    {
      byte[] bufferPalette = reader.ReadBytes(256 * sizeof(UInt32));
      Int32 sizeUncompressed = reader.ReadInt32();

      // TODO(adm244): check compressed size; or read buffer first and then decode
      Int32 sizeCompressed = reader.ReadInt32();

      byte[] bufferPixels = AGSCompression.ReadLZ77(reader, sizeUncompressed, bytesPerPixel, out int width, out int height);
      PixelFormat format = PixelFormatExtension.FromBytesPerPixel(bytesPerPixel);

      PixelFormat paletteFormat = PixelFormat.Argb32;
      if (format == PixelFormat.Indexed)
        paletteFormat = PixelFormat.Argb6666;

      Palette palette = Palette.FromBuffer(bufferPalette, paletteFormat);

      if (format == PixelFormat.Indexed)
        return new Bitmap(width, height, bufferPixels, format, palette);

      return new Bitmap(width, height, bufferPixels, format);
    }

    public static void WriteLZ77Image(BinaryWriter writer, Bitmap image, int bytesPerPixel)
    {
      // CHECK(adm244): palette format in room files
      PixelFormat paletteFormat = PixelFormat.Argb32;
      if (image.Format == PixelFormat.Indexed)
        paletteFormat = PixelFormat.Argb6666;

      Palette palette = image.Palette.HasValue ? image.Palette.Value : AGSSpriteSet.DefaultPalette;
      byte[] paletteBuffer = palette.ToBuffer(paletteFormat);
      writer.Write((byte[])paletteBuffer);

      //NOTE(adm244): convert bitmap to match requested bytesPerPixel
      PixelFormat targetFormat = PixelFormatExtension.FromBytesPerPixel(bytesPerPixel);
      if (image.Format != targetFormat)
        image = image.Convert(targetFormat);

      byte[] pixels = image.GetPixels();
      Debug.Assert(pixels.Length == (image.Width * image.Height * bytesPerPixel));

      byte[] buffer = PreAppendImageSize(image.Width, image.Height, bytesPerPixel, pixels);
      byte[] bufferCompressed = AGSCompression.LZ77Compress(buffer);

      writer.Write((UInt32)buffer.Length);
      writer.Write((UInt32)bufferCompressed.Length);
      writer.Write((byte[])bufferCompressed);
    }

    //TODO(adm244): this method looks suspicious, investigate
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
