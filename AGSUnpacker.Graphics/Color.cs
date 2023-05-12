﻿using System;

using AGSUnpacker.Graphics.Formats;

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

    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }

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
          return colors.ToRgb565();

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

    private static byte[] ToRgb565(this Color[] colors)
    {
      int bytesPerPixel = 2;

      byte[] buffer = new byte[colors.Length * bytesPerPixel];

      for (int i = 0; i < colors.Length; ++i)
      {
        int red   = colors[i].R;
        int green = colors[i].G;
        int blue  = colors[i].B;

        red   = (int)((red   / 256f) * 32);
        green = (int)((green / 256f) * 64);
        blue  = (int)((blue  / 256f) * 32);

        // NOTE(adm244): little-endian bgr565 format
        UInt16 value = (UInt16)((byte)(blue << 11) | (byte)(green << 5) | (byte)(red));

        buffer[bytesPerPixel * i + 0] = (byte)(value >> 8);
        buffer[bytesPerPixel * i + 1] = (byte)(value);
      }

      return buffer;
    }

    private static byte[] ToRgb24(this Color[] colors, PixelFormat format)
    {
      int bytesPerPixel = format.GetBytesPerPixel();
      if (bytesPerPixel != 3)
        throw new ArgumentException("Invalid color format for RGB!");

      byte[] buffer = new byte[colors.Length * bytesPerPixel];

      for (int i = 0; i < colors.Length; ++i)
      {
        int red   = colors[i].R;
        int green = colors[i].G;
        int blue  = colors[i].B;

        if (format == PixelFormat.Rgb666)
        {
          //TODO(adm244): consider moving this into MathUtils or something
          red   = (int)((red   / 256f) * 64f);
          green = (int)((green / 256f) * 64f);
          blue  = (int)((blue  / 256f) * 64f);
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
        throw new ArgumentException("Invalid color format for RGBA!");

      byte[] buffer = new byte[colors.Length * bytesPerPixel];

      for (int i = 0; i < colors.Length; ++i)
      {
        int red   = colors[i].R;
        int green = colors[i].G;
        int blue  = colors[i].B;
        int alpha = colors[i].A;

        if (format == PixelFormat.Argb6666)
        {
          red   = (int)((red   / 256f) * 64f);
          green = (int)((green / 256f) * 64f);
          blue = (int)((blue / 256f) * 64f);
          alpha = (int)((alpha / 256f) * 64f);
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
