using System;
using System.Runtime.InteropServices;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AGSUnpacker.Lib.Extensions
{
  public static class ImageExtension
  {
    public static byte[] ConvertIndexedToBgra32(byte[] buffer, Bgra32[] palette)
    {
      byte[] newBuffer = new byte[buffer.Length * sizeof(Int32)];

      // HACK(adm244): treat palette index 0 as transparent color
      // some palette colors are the same as color 0, assuming color 0 is transparent
      palette[0].A = 0;

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

    public static Image<Bgra32> LoadIndexedAsBgra32(byte[] buffer, int width, int height, Bgra32[] palette)
    {
      byte[] pixelData = ConvertIndexedToBgra32(buffer, palette);
      return Image.LoadPixelData<Bgra32>(pixelData, width, height);
    }

    public static Image<Bgra32> LoadPixelDataAsBgra32<TPixel>(byte[] buffer, int width, int height)
      where TPixel : unmanaged, IPixel<TPixel>
    {
      Image sourceImage = Image.LoadPixelData<TPixel>(buffer, width, height);
      return sourceImage.CloneAs<Bgra32>();
    }

    public static byte[] GetPixels(this Image image)
    {
      switch (image.PixelType.BitsPerPixel)
      {
        case 32:
          (image as Image<Bgra32>).TryGetSinglePixelSpan(out var span);
          return MemoryMarshal.AsBytes(span).ToArray();

        default:
          throw new NotSupportedException();
      }
    }
  }
}
