using System;

using AGSUnpacker.Graphics.Formats;

namespace AGSUnpacker.Graphics
{
  public abstract class BitmapInstance
  {
    protected byte[] _buffer;

    public PixelFormat Format { get; private set; }
    public Palette Palette { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public static BitmapInstance Create(string filepath)
    {
      return new BitmapInstanceImpl(filepath);
    }

    public static BitmapInstance Create(int width, int height, byte[] buffer, PixelFormat format, Palette? palette = null)
    {
      return new BitmapInstanceImpl(width, height, buffer, format, palette);
    }

    protected BitmapInstance()
    {
    }

    public BitmapInstance(string filepath)
    {
      BitmapInstanceInfo bitmapInfo = LoadFromFile(filepath);
      Initialize(bitmapInfo);
    }

    public BitmapInstance(int width, int height, byte[] buffer, PixelFormat format, Palette? palette = null)
    {
      Initialize(width, height, buffer, format, palette);
    }

    public abstract BitmapInstance Convert(PixelFormat format);
    public abstract byte[] GetPixels();
    public abstract void Save(string filepath, ImageFormat format);

    protected abstract BitmapInstanceInfo LoadFromFile(string filepath);

    protected void Initialize(BitmapInstanceInfo bitmap)
    {
      Initialize(bitmap.Width, bitmap.Height, bitmap.Buffer, bitmap.Format, bitmap.Palette);
    }

    protected void Initialize(int width, int height, byte[] buffer, PixelFormat format, Palette? palette = null)
    {
      _buffer = buffer;

      Width = width;
      Height = height;
      Format = format;

      if (palette.HasValue && format == PixelFormat.Indexed)
        Palette = palette.Value;
      else if (!palette.HasValue && format == PixelFormat.Indexed)
        throw new ArgumentNullException(nameof(palette));
      else if (palette.HasValue)
        throw new ArgumentException(null, nameof(palette));
    }

    protected struct BitmapInstanceInfo
    {
      public int Width;
      public int Height;

      public Palette? Palette;

      // TODO(adm244): make a PixelBuffer struct?
      public PixelFormat Format;
      public byte[] Buffer;
    }
  }
}
