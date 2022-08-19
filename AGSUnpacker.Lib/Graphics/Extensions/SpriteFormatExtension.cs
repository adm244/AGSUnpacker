using AGSUnpacker.Graphics.Formats;

namespace AGSUnpacker.Lib.Graphics.Extensions
{
  public static partial class SpriteFormatExtension
  {
    public static PixelFormat? ToPixelFormat(this SpriteFormat format)
    {
      switch (format)
      {
        case SpriteFormat.PaletteRGB565:
          return PixelFormat.Rgb565;
        case SpriteFormat.PaletteRGB24:
          return PixelFormat.Rgb24;
        case SpriteFormat.PaletteARGB32:
          return PixelFormat.Argb32;

        case SpriteFormat.Default:
        default:
          return null;
      }
    }
  }
}
