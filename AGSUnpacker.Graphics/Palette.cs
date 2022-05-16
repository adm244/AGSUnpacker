using System;

using AGSUnpacker.Graphics.Formats;

namespace AGSUnpacker.Graphics
{
  public struct Palette
  {
    public Palette(Color[] entries, PixelFormat? sourceFormat = null)
    {
      Entries = new Color[entries.Length];
      entries.CopyTo(Entries, 0);

      SourceFormat = sourceFormat;

      // FIX(adm244): just use tRNS chunk in png
      //
      // HACK(adm244): treat palette index 0 as transparent color
      // some palette colors are the same as color 0, assuming color 0 is transparent
      //Entries[0] = new Color(Entries[0].R, Entries[0].G, Entries[0].B, 0);
    }

    public PixelFormat? SourceFormat { get; }
    public Color[] Entries { get; }
    public int Length => Entries.Length;
    public bool Empty => Entries.Length == 0;

    public Color this[int index] => Entries[index];

    public byte[] ToBuffer()
    {
      if (!SourceFormat.HasValue)
        throw new ArgumentNullException(nameof(SourceFormat));

      return ToBuffer(SourceFormat.Value);
    }

    public byte[] ToBuffer(PixelFormat format)
    {
      return Entries.ToBuffer(format);
    }

    //public byte[] ToBuffer(PixelFormat format)
    //{
    //  switch (format)
    //  {
    //    case PixelFormat.Rgb666:
    //    case PixelFormat.Rgb24:
    //      return ToRgb(format);
    //
    //    case PixelFormat.Argb6666:
    //    case PixelFormat.Argb32:
    //      return ToRgba(format);
    //
    //    default:
    //      throw new NotSupportedException("Not supported palette format!");
    //  }
    //}
    //
    //private byte[] ToRgb(PixelFormat format)
    //{
    //  int bytesPerPixel = format.GetBytesPerPixel();
    //  if (bytesPerPixel != 3)
    //    throw new ArgumentException("Invalid color format for RGB!");
    //
    //  byte[] buffer = new byte[Entries.Length * bytesPerPixel];
    //
    //  for (int i = 0; i < Entries.Length; ++i)
    //  {
    //    int red   = Entries[i].R;
    //    int green = Entries[i].G;
    //    int blue  = Entries[i].B;
    //
    //    if (format == PixelFormat.Rgb666)
    //    {
    //      //TODO(adm244): consider moving this into MathUtils or something
    //      red   = (int)((red   / 256f) * 64f);
    //      green = (int)((green / 256f) * 64f);
    //      blue  = (int)((blue  / 256f) * 64f);
    //    }
    //
    //    buffer[bytesPerPixel * i + 0] = (byte)red;
    //    buffer[bytesPerPixel * i + 1] = (byte)green;
    //    buffer[bytesPerPixel * i + 2] = (byte)blue;
    //  }
    //
    //  return buffer;
    //}
    //
    //private byte[] ToRgba(PixelFormat format)
    //{
    //  int bytesPerPixel = format.GetBytesPerPixel();
    //  if (bytesPerPixel != 4)
    //    throw new ArgumentException("Invalid color format for RGBA!");
    //
    //  byte[] buffer = new byte[Entries.Length * bytesPerPixel];
    //
    //  for (int i = 0; i < Entries.Length; ++i)
    //  {
    //    int red   = Entries[i].R;
    //    int green = Entries[i].G;
    //    int blue  = Entries[i].B;
    //    int alpha = Entries[i].A;
    //
    //    if (format == PixelFormat.Argb6666)
    //    {
    //      red   = (int)((red   / 256f) * 64f);
    //      green = (int)((green / 256f) * 64f);
    //      blue  = (int)((blue  / 256f) * 64f);
    //      alpha = (int)((alpha / 256f) * 64f);
    //    }
    //
    //    buffer[bytesPerPixel * i + 0] = (byte)red;
    //    buffer[bytesPerPixel * i + 1] = (byte)green;
    //    buffer[bytesPerPixel * i + 2] = (byte)blue;
    //    buffer[bytesPerPixel * i + 3] = (byte)alpha;
    //  }
    //
    //  return buffer;
    //}

    public static Palette FromBuffer(byte[] buffer, PixelFormat format, bool discardAlpha = true)
    {
      switch (format)
      {
        case PixelFormat.Rgb666:
        case PixelFormat.Rgb24:
          return FromRgb(buffer, format);

        case PixelFormat.Argb6666:
        case PixelFormat.Argb32:
          return FromRgba(buffer, format, discardAlpha);

        default:
          throw new NotSupportedException("Not supported palette format!");
      }
    }

    private static Palette FromRgb(byte[] buffer, PixelFormat format)
    {
      int bytesPerPixel = format.GetBytesPerPixel();
      if (bytesPerPixel != 3)
        throw new ArgumentException("Invalid color format for RGB!");

      int count = (buffer.Length / bytesPerPixel);
      Color[] colors = new Color[count];
      for (int i = 0; i < colors.Length; ++i)
      {
        byte red   = buffer[bytesPerPixel * i + 0];
        byte green = buffer[bytesPerPixel * i + 1];
        byte blue  = buffer[bytesPerPixel * i + 2];

        if (format == PixelFormat.Rgb666)
        {
          blue  = (byte)((blue  / 64f) * 256f);
          green = (byte)((green / 64f) * 256f);
          red   = (byte)((red   / 64f) * 256f);
        }

        colors[i] = new Color(red, green, blue);
      }

      return new Palette(colors, format);
    }

    private static Palette FromRgba(byte[] buffer, PixelFormat format, bool discardAlpha = true)
    {
      int bytesPerPixel = format.GetBytesPerPixel();
      if (bytesPerPixel != 4)
        throw new ArgumentException("Invalid color format for RGBA!");

      int count = (buffer.Length / bytesPerPixel);
      Color[] colors = new Color[count];
      for (int i = 0; i < colors.Length; ++i)
      {
        byte red   = buffer[bytesPerPixel * i + 0];
        byte green = buffer[bytesPerPixel * i + 1];
        byte blue  = buffer[bytesPerPixel * i + 2];
        byte alpha = buffer[bytesPerPixel * i + 3];

        if (format == PixelFormat.Argb6666)
        {
          red   = (byte)((red   / 64f) * 256f);
          green = (byte)((green / 64f) * 256f);
          blue  = (byte)((blue  / 64f) * 256f);
          alpha = (byte)((alpha / 64f) * 256f);
        }

        if (discardAlpha)
          alpha = 255;

        colors[i] = new Color(red, green, blue, alpha);
      }

      return new Palette(colors, format);
    }
  }
}
