using System;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace AGSUnpacker.Graphics
{
  internal static class BitmapInstanceExtension
  {
    internal static SixLabors.ImageSharp.Color[] ConvertToColor(Palette palette)
    {
      var colors = new SixLabors.ImageSharp.Color[palette.Length];

      for (int i = 0; i < palette.Length; ++i)
      {
        colors[i] = SixLabors.ImageSharp.Color.FromRgba(palette[i].R, palette[i].G, palette[i].B, palette[i].A);
      }

      return colors;
    }

    internal static BmpBitsPerPixel GetBmpBitsPerPixel(PixelFormat format)
    {
      switch (format)
      {
        case PixelFormat.Rgb565:
          return BmpBitsPerPixel.Pixel16;
        case PixelFormat.Rgb24:
          return BmpBitsPerPixel.Pixel24;

        default:
          throw new NotSupportedException();
      }
    }

    internal static void SaveAsBmp(this BitmapInstanceImpl bitmap, string filepath)
    {
      if (bitmap.Format == PixelFormat.Argb32)
        throw new InvalidOperationException();

      BmpEncoder encoder = new BmpEncoder();

      if (bitmap.Format == PixelFormat.Indexed)
      {
        SixLabors.ImageSharp.Color[] palette = ConvertToColor(bitmap.Palette);
        encoder.Quantizer = new PaletteQuantizer(palette);
        encoder.BitsPerPixel = BmpBitsPerPixel.Pixel8;
      }
      else
      {
        encoder.BitsPerPixel = GetBmpBitsPerPixel(bitmap.Format);
      }

      bitmap.Instance.SaveAsBmp(filepath, encoder);
    }

    internal static void SaveAsPng(this BitmapInstanceImpl bitmap, string filepath)
    {
      if (bitmap.Format != PixelFormat.Argb32)
        throw new InvalidOperationException();

      PngEncoder encoder = new PngEncoder();

      encoder.ColorType = PngColorType.RgbWithAlpha;
      encoder.BitDepth = PngBitDepth.Bit8;
      encoder.ChunkFilter = PngChunkFilter.ExcludeAll;

      bitmap.Instance.SaveAsPng(filepath, encoder);
    }

    internal static PixelFormat GetPixelFormat(Image image)
    {
      int bytesPerPixel = image.PixelType.BitsPerPixel / 8;
      return PixelFormatExtension.FromBytesPerPixel(bytesPerPixel);
    }

    internal static Image<Bgra32> LoadFromFileAsBgra32(string filepath, out PixelFormat format)
    {
      Image image = Image.Load(filepath);
      format = GetPixelFormat(image);
      return image.CloneAs<Bgra32>();
    }

    internal static byte[] ConvertIndexedToBgra32(byte[] buffer, Palette palette)
    {
      byte[] newBuffer = new byte[buffer.Length * sizeof(Int32)];

      for (int i = 0; i < buffer.Length; ++i)
      {
        int index = buffer[i];

        newBuffer[i * 4 + 0] = palette[index].B;
        newBuffer[i * 4 + 1] = palette[index].G;
        newBuffer[i * 4 + 2] = palette[index].R;
        newBuffer[i * 4 + 3] = palette[index].A;
      }

      return newBuffer;
    }

    internal static Image<Bgra32> CreateImageBgra32(int width, int height, byte[] buffer, PixelFormat format)
    {
      Image image = CreateImage(width, height, buffer, format);
      return image.CloneAs<Bgra32>();
    }

    internal static Image<Bgra32> CreateImageBgra32(int width, int height, byte[] buffer, PixelFormat format, Palette palette)
    {
      byte[] bufferConverted = ConvertToBgra32Palette(buffer, format, palette);
      return Image.LoadPixelData<Bgra32>(bufferConverted, width, height);
    }

    internal static Image CreateImage(int width, int height, byte[] buffer, PixelFormat format)
    {
      switch (format)
      {
        case PixelFormat.Rgb565:
          return Image.LoadPixelData<Bgr565>(buffer, width, height);

        case PixelFormat.Rgb24:
          return Image.LoadPixelData<Bgr24>(buffer, width, height);

        case PixelFormat.Argb32:
          return Image.LoadPixelData<Bgra32>(buffer, width, height);

        default:
          throw new NotSupportedException();
      }
    }

    internal static byte[] ConvertToBgra32Palette(byte[] buffer, PixelFormat format, Palette palette)
    {
      switch (format)
      {
        case PixelFormat.Indexed:
          return ConvertIndexedToBgra32(buffer, palette);

        default:
          throw new NotSupportedException();
      }
    }

    //public static Image<Bgra32> LoadIndexedAsBgra32(byte[] buffer, int width, int height, Palette palette)
    //{
    //  byte[] pixelData = ConvertIndexedToBgra32(buffer, palette);
    //  return Image.LoadPixelData<Bgra32>(pixelData, width, height);
    //}

    //public static Image<Bgra32> LoadPixelDataAsBgra32<TPixel>(byte[] buffer, int width, int height)
    //  where TPixel : unmanaged, IPixel<TPixel>
    //{
    //  Image sourceImage = Image.LoadPixelData<TPixel>(buffer, width, height);
    //  return sourceImage.CloneAs<Bgra32>();
    //}

    //public static byte[] GetPixels(this Image image)
    //{
    //  switch (image.PixelType.BitsPerPixel)
    //  {
    //    case 32:
    //      (image as Image<Bgra32>).TryGetSinglePixelSpan(out var span);
    //      return MemoryMarshal.AsBytes(span).ToArray();
    //
    //    default:
    //      throw new NotSupportedException();
    //  }
    //}
  }
}
