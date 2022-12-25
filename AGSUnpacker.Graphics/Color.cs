using System;

using AGSUnpacker.Graphics.Formats;
using AGSUnpacker.Shared.Utils;

namespace AGSUnpacker.Graphics
{
  public struct Color
  {
    public Color(byte r, byte g, byte b)
      : this(r, g, b, 255)
    {
    }

    public Color(byte r, byte g, byte b, byte a)
    {
      R = r;
      G = g;
      B = b;
      A = a;
    }

    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    public byte A { get; }

    public int ToRgba32()
    {
      return (A << 24) | (B << 16) | (G << 8) | R;
    }

    public static Color FromRgba32(int rgba32)
    {
      byte red   = (byte)((rgba32 >>  0) & 0xFF);
      byte green = (byte)((rgba32 >>  8) & 0xFF);
      byte blue  = (byte)((rgba32 >> 16) & 0xFF);
      byte alpha = (byte)((rgba32 >> 24) & 0xFF);

      return new Color(red, green, blue, alpha);
    }
  }

  public static class ColorExtension
  {
    public static byte[] ToBuffer(this Color[] colors, PixelFormat format)
    {
      switch (format)
      {
        case PixelFormat.Rgb565:
          return colors.ToRgb16();

        case PixelFormat.Rgb666:
        case PixelFormat.Rgb24:
          return colors.ToRgb24(format);

        case PixelFormat.Argb6666:
        case PixelFormat.Argb32:
          return colors.ToRgba32(format);

        default:
          throw new NotSupportedException("Not supported palette format!");
      }
    }

    private static byte[] ToRgb16(this Color[] colors)
    {
      int bytesPerPixel = 2;

      byte[] buffer = new byte[colors.Length * bytesPerPixel];

      for (int i = 0; i < colors.Length; ++i)
      {
        int red   = colors[i].R;
        int green = colors[i].G;
        int blue  = colors[i].B;

        red   = Utils.Remap(255, red,   31);
        green = Utils.Remap(255, green, 63);
        blue  = Utils.Remap(255, blue,  31);

        // NOTE(adm244): little-endian bgr565 format
        UInt16 value = (UInt16)(((blue & 0xFF) << 11) | ((green & 0xFF) << 5) | (red & 0xFF));

        buffer[bytesPerPixel * i + 0] = (byte)(value >> 8);
        buffer[bytesPerPixel * i + 1] = (byte)(value);
      }

      return buffer;
    }

    private static byte[] ToRgb24(this Color[] colors, PixelFormat format)
    {
      int bytesPerPixel = format.GetBytesPerPixel();
      if (bytesPerPixel != 3)
        throw new ArgumentException("Invalid color format for RGB (24-bit)!");

      byte[] buffer = new byte[colors.Length * bytesPerPixel];

      for (int i = 0; i < colors.Length; ++i)
      {
        int red   = colors[i].R;
        int green = colors[i].G;
        int blue  = colors[i].B;

        if (format == PixelFormat.Rgb666)
        {
          red   = Utils.Remap(255, red,   63);
          green = Utils.Remap(255, green, 63);
          blue  = Utils.Remap(255, blue,  63);
        }

        buffer[bytesPerPixel * i + 0] = (byte)red;
        buffer[bytesPerPixel * i + 1] = (byte)green;
        buffer[bytesPerPixel * i + 2] = (byte)blue;
      }

      return buffer;
    }

    private static byte[] ToRgba32(this Color[] colors, PixelFormat format)
    {
      int bytesPerPixel = format.GetBytesPerPixel();
      if (bytesPerPixel != 4)
        throw new ArgumentException("Invalid color format for RGBA (32-bit)!");

      byte[] buffer = new byte[colors.Length * bytesPerPixel];

      for (int i = 0; i < colors.Length; ++i)
      {
        int red   = colors[i].R;
        int green = colors[i].G;
        int blue  = colors[i].B;
        int alpha = colors[i].A;

        if (format == PixelFormat.Argb6666)
        {
          red   = Utils.Remap(255, red,   63);
          green = Utils.Remap(255, green, 63);
          blue  = Utils.Remap(255, blue,  63);
          alpha = Utils.Remap(255, alpha, 63);
        }

        buffer[bytesPerPixel * i + 0] = (byte)red;
        buffer[bytesPerPixel * i + 1] = (byte)green;
        buffer[bytesPerPixel * i + 2] = (byte)blue;
        buffer[bytesPerPixel * i + 3] = (byte)alpha;
      }

      return buffer;
    }
  }
}
