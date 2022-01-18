using System;
using System.Runtime.InteropServices;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AGSUnpacker.Graphics
{
  public class BitmapInstanceImpl : BitmapInstance
  {
    public BitmapInstanceImpl(string filepath)
    {
      Instance = BitmapInstanceExtension.LoadFromFileAsBgra32(filepath, out PixelFormat format);
      Format = format;
    }

    public BitmapInstanceImpl(int width, int height, byte[] buffer, PixelFormat format)
    {
      Instance = BitmapInstanceExtension.CreateImageBgra32(width, height, buffer, format);
      Format = format;
    }

    public BitmapInstanceImpl(int width, int height, byte[] buffer, PixelFormat format, Palette palette)
    {
      Instance = BitmapInstanceExtension.CreateImageBgra32(width, height, buffer, format, palette);
      Format = format;

      // TODO(adm244): create a copy of these
      Palette = palette;
      Indices = buffer;
    }

    private BitmapInstanceImpl(BitmapInstanceImpl bitmap, PixelFormat format)
    {
      Instance = bitmap.Instance.CloneAs<Bgra32>();
      Format = format;

      // TODO(adm244): create a copy of these
      Palette = bitmap.Palette;
      Indices = bitmap.Indices;
    }

    internal Image Instance { get; }
    public PixelFormat Format { get; }
    public int Width => Instance.Width;
    public int Height => Instance.Height;

    internal Palette Palette { get; }
    private byte[] Indices { get; }

    public BitmapInstance Convert(PixelFormat format)
    {
      return new BitmapInstanceImpl(this, format);
    }

    public byte[] GetPixels()
    {
      switch (Format)
      {
        case PixelFormat.Indexed:
          return Indices;

        case PixelFormat.Rgb565:
        {
          (Instance as Image<Bgr565>).TryGetSinglePixelSpan(out Span<Bgr565> span);
          return MemoryMarshal.AsBytes(span).ToArray();
        }

        case PixelFormat.Rgb24:
        {
          (Instance as Image<Bgr24>).TryGetSinglePixelSpan(out Span<Bgr24> span);
          return MemoryMarshal.AsBytes(span).ToArray();
        }

        case PixelFormat.Argb32:
        {
          (Instance as Image<Bgra32>).TryGetSinglePixelSpan(out Span<Bgra32> span);
          return MemoryMarshal.AsBytes(span).ToArray();
        }

        default:
          throw new NotSupportedException();
      }
    }

    public void Save(string filepath, ImageFormat format)
    {
      switch (format)
      {
        case ImageFormat.Bmp:
          this.SaveAsBmp(filepath);
          break;

        case ImageFormat.Png:
          this.SaveAsPng(filepath);
          break;

        default:
          throw new NotSupportedException();
      }
    }
  }
}
