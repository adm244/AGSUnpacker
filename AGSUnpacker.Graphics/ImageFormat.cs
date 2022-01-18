using System;

namespace AGSUnpacker.Graphics
{
  public enum ImageFormat
  {
    Bmp,
    Png
  }

  public static class ImageFormatExtension
  {
    public static string GetExtension(this ImageFormat format)
    {
      switch (format)
      {
        case ImageFormat.Bmp:
          return ".bmp";
        case ImageFormat.Png:
          return ".png";

        default:
          throw new NotSupportedException();
      }
    }
  }
}
