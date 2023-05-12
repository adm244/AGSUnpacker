using System;

using AGSUnpacker.Graphics.Formats;

namespace AGSUnpacker.Graphics
{
  public class BitmapInstanceImpl : BitmapInstance
  {
    public BitmapInstanceImpl(string filepath)
      : base(filepath)
    {
    }

    public BitmapInstanceImpl(int width, int height, byte[] buffer, PixelFormat format, Palette? palette = null)
      : base(width, height, buffer, format, palette)
    {
      throw new NotImplementedException();
    }

    public override BitmapInstance Convert(PixelFormat format, bool discardAlpha)
    {
      throw new NotImplementedException();
    }

    public override byte[] GetPixels()
    {
      throw new NotImplementedException();
    }

    public override void Save(string filepath, ImageFormat format)
    {
      throw new NotImplementedException();
    }

    protected override BitmapInstanceInfo LoadFromFile(string filepath)
    {
      Bitmap bitmap = PngImage.ReadFile(filepath);
      return new BitmapInstanceInfo
      {
        Width = bitmap.Width,
        Height = bitmap.Height,
        Palette = bitmap.Palette,
        Format = bitmap.Format,
        Buffer = bitmap.GetPixels()
      };
    }
  }
}
