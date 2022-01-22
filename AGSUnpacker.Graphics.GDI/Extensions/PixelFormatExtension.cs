using System;

using AGSUnpacker.Graphics.Formats;

namespace AGSUnpacker.Graphics.GDI.Extensions
{
  internal static partial class PixelFormatExtension
  {
    internal static System.Drawing.Imaging.PixelFormat ToGDIFormat(this PixelFormat format)
    {
      switch (format)
      {
        case PixelFormat.Indexed:
          return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;

        case PixelFormat.Rgb565:
          return System.Drawing.Imaging.PixelFormat.Format16bppRgb565;

        case PixelFormat.Rgb24:
          return System.Drawing.Imaging.PixelFormat.Format24bppRgb;

        case PixelFormat.Argb32:
          return System.Drawing.Imaging.PixelFormat.Format32bppArgb;

        default:
          throw new NotSupportedException();
      }
    }
  }

  internal static class PixelFormatGDIExtension
  {
    internal static PixelFormat ToAGSFormat(this System.Drawing.Imaging.PixelFormat format)
    {
      switch (format)
      {
        case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
          return PixelFormat.Indexed;

        case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
          return PixelFormat.Rgb565;

        case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
          return PixelFormat.Rgb24;

        case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
          return PixelFormat.Argb32;

        default:
          throw new NotSupportedException();
      }
    }
  }
}
