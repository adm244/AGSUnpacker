using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace AGSUnpackerSharp.Extensions
{
  public static class BitmapExtension
  {
    public static bool HasAlphaChannel(this Bitmap bitmap)
    {
      switch (bitmap.PixelFormat)
      {
        case PixelFormat.Alpha:
        case PixelFormat.PAlpha:
        case PixelFormat.Format16bppArgb1555:
        case PixelFormat.Format32bppArgb:
        case PixelFormat.Format32bppPArgb:
        case PixelFormat.Format64bppArgb:
        case PixelFormat.Format64bppPArgb:
          return true;

        default:
          return false;
      }
    }

    //TODO(adm244): maybe this shouldn't be here since we hardcore format values
    public static PixelFormat GetPixelFormat(int bytesPerPixel)
    {
      switch (bytesPerPixel)
      {
        case 1:
          return PixelFormat.Format8bppIndexed;
        case 2:
          return PixelFormat.Format16bppRgb565;
        case 3:
          return PixelFormat.Format24bppRgb;
        case 4:
          return PixelFormat.Format32bppArgb;

        default:
          return PixelFormat.Undefined;
      }
    }

    public static int GetBytesPerPixel(this Bitmap bitmap)
    {
      return (Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8);
    }

    public static byte[] GetPixels(this Bitmap bitmap)
    {
      int bytesPerPixel = bitmap.GetBytesPerPixel();
      int imageSize = (bitmap.Width * bitmap.Height * bytesPerPixel);
      byte[] pixels = new byte[imageSize];

      Rectangle lockRegion = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
      BitmapData lockData = bitmap.LockBits(lockRegion, ImageLockMode.ReadOnly, bitmap.PixelFormat);

      IntPtr p = lockData.Scan0;
      for (int row = 0; row < bitmap.Height; ++row)
      {
        Marshal.Copy(p, pixels, row * bitmap.Width * bytesPerPixel, bitmap.Width * bytesPerPixel);
        p = new IntPtr(p.ToInt64() + lockData.Stride);
      }
      bitmap.UnlockBits(lockData);

      return pixels;
    }

    public static void SetPixels(this Bitmap bitmap, byte[] buffer)
    {
      if (buffer == null)
        throw new InvalidDataException("Buffer is null!");

      Rectangle lockRegion = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
      BitmapData lockData = bitmap.LockBits(lockRegion, ImageLockMode.WriteOnly, bitmap.PixelFormat);
      int bytesPerPixel = bitmap.GetBytesPerPixel();

      IntPtr p = lockData.Scan0;
      for (int row = 0; row < bitmap.Height; ++row)
      {
        Marshal.Copy(buffer, row * bitmap.Width * bytesPerPixel, p, bitmap.Width * bytesPerPixel);
        p = new IntPtr(p.ToInt64() + lockData.Stride);
      }
      bitmap.UnlockBits(lockData);
    }

    public static void SetPalette(this Bitmap bitmap, Color[] palette)
    {
      if (palette == null)
        throw new InvalidDataException("Palette is null!");

      ColorPalette currentPalette = bitmap.Palette;
      for (int j = 0; j < currentPalette.Entries.Length; ++j)
      {
        currentPalette.Entries[j] = palette[j];
      }
      bitmap.Palette = currentPalette;
    }

    public static Bitmap Convert(this Bitmap bitmap, PixelFormat format)
    {
      Rectangle copyRegion = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
      return bitmap.Clone(copyRegion, format);
    }
  }
}
