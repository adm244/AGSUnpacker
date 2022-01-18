using System;

namespace AGSUnpacker.Graphics
{
  public enum PixelFormat
  {
    Indexed,  // 1 byte
    Rgb565,   // 2 bytes
    Rgb666,   // 3 bytes
    Rgb24,    // 3 bytes
    Argb6666, // 4 bytes
    Argb32    // 4 bytes
  }

  public static partial class PixelFormatExtension
  {
    public static ImageFormat GetImageFormat(this PixelFormat format)
    {
      switch (format)
      {
        case PixelFormat.Indexed:
        case PixelFormat.Rgb565:
        case PixelFormat.Rgb24:
        case PixelFormat.Argb32:
          return ImageFormat.Png;

        default:
          throw new NotSupportedException();
      }
    }

    public static PixelFormat FromBytesPerPixel(int bytesPerPixel)
    {
      switch (bytesPerPixel)
      {
        case 1:
          return PixelFormat.Indexed;
        case 2:
          return PixelFormat.Rgb565;
        case 3:
          return PixelFormat.Rgb24;
        case 4:
          return PixelFormat.Argb32;

        default:
          throw new NotSupportedException();
      }
    }

    public static int GetBytesPerPixel(this PixelFormat format)
    {
      switch (format)
      {
        case PixelFormat.Indexed:
          return 1;
        case PixelFormat.Rgb565:
          return 2;
        case PixelFormat.Rgb666:
        case PixelFormat.Rgb24:
          return 3;
        case PixelFormat.Argb6666:
        case PixelFormat.Argb32:
          return 4;

        default:
          throw new NotSupportedException();
      }
    }
  }
}
