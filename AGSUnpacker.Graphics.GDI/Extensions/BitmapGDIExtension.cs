using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AGSUnpacker.Graphics.GDI.Extensions
{
  internal static class BitmapGDIExtension
  {
    //TODO(adm244): maybe this shouldn't be here since we hardcore format values
    internal static System.Drawing.Imaging.PixelFormat GetPixelFormat(int bytesPerPixel)
    {
      switch (bytesPerPixel)
      {
        case 1:
          return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
        case 2:
          return System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
        case 3:
          return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
        case 4:
          return System.Drawing.Imaging.PixelFormat.Format32bppArgb;

        default:
          return System.Drawing.Imaging.PixelFormat.Undefined;
      }
    }

    internal static int GetBytesPerPixel(this System.Drawing.Bitmap bitmap)
    {
      return GetBytesPerPixel(bitmap.PixelFormat);
    }

    internal static int GetBytesPerPixel(System.Drawing.Imaging.PixelFormat format)
    {
      return System.Drawing.Image.GetPixelFormatSize(format) / 8;
    }

    internal static byte[] GetPixels(this System.Drawing.Bitmap bitmap)
    {
      return GetPixels(bitmap, bitmap.PixelFormat);
    }

    internal static byte[] GetPixels(this System.Drawing.Bitmap bitmap, System.Drawing.Imaging.PixelFormat format)
    {
      int bytesPerPixel = GetBytesPerPixel(format);
      int imageSize = (bitmap.Width * bitmap.Height * bytesPerPixel);
      byte[] pixels = new byte[imageSize];

      System.Drawing.Rectangle lockRegion = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
      System.Drawing.Imaging.BitmapData lockData = bitmap.LockBits(lockRegion, System.Drawing.Imaging.ImageLockMode.ReadOnly, format);

      IntPtr p = lockData.Scan0;
      for (int row = 0; row < bitmap.Height; ++row)
      {
        Marshal.Copy(p, pixels, row * bitmap.Width * bytesPerPixel, bitmap.Width * bytesPerPixel);
        p = new IntPtr(p.ToInt64() + lockData.Stride);
      }
      bitmap.UnlockBits(lockData);

      return pixels;
    }

    internal static void SetPixels(this System.Drawing.Bitmap bitmap, byte[] buffer)
    {
      SetPixels(bitmap, buffer, bitmap.PixelFormat);
    }

    internal static void SetPixels(this System.Drawing.Bitmap bitmap, byte[] buffer, System.Drawing.Imaging.PixelFormat format)
    {
      if (buffer == null)
        throw new ArgumentNullException(nameof(buffer));

      System.Drawing.Rectangle lockRegion = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
      System.Drawing.Imaging.BitmapData lockData = bitmap.LockBits(lockRegion, System.Drawing.Imaging.ImageLockMode.WriteOnly, format);

      int bytesPerPixel = GetBytesPerPixel(format);
      IntPtr p = lockData.Scan0;
      for (int row = 0; row < bitmap.Height; ++row)
      {
        Marshal.Copy(buffer, row * bitmap.Width * bytesPerPixel, p, bitmap.Width * bytesPerPixel);
        p = new IntPtr(p.ToInt64() + lockData.Stride);
      }
      bitmap.UnlockBits(lockData);
    }

    internal static void SetPalette(this System.Drawing.Bitmap bitmap, Palette palette)
    {
      Debug.Assert(palette.Entries != null);
      Debug.Assert(palette.Entries.Length > 0);
      Debug.Assert(palette.Entries.Length == palette.Length);

      System.Drawing.Imaging.ColorPalette currentPalette = bitmap.Palette;
      for (int i = 0; i < currentPalette.Entries.Length; ++i)
      {
        Color color = palette[i];
        currentPalette.Entries[i] = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
      }
      bitmap.Palette = currentPalette;
    }

    internal static System.Drawing.Bitmap Convert(this System.Drawing.Bitmap bitmap, System.Drawing.Imaging.PixelFormat format)
    {
      System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height, format);
      byte[] pixels = bitmap.GetPixels(format);
      newBitmap.SetPixels(pixels, format);

      return newBitmap;
    }
  }
}
