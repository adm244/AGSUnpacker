using System;

using AGSUnpacker.Lib.Extensions;

using SixLabors.ImageSharp.PixelFormats;

namespace AGSUnpacker.Lib.Graphics
{
  public enum PixelFormat
  {
    Undefined = 0,
    Indexed,
    Rgb565,
    Rgb24,
    Argb32
  }

  public class Image
  {
    public Bgra32[] Palette { get; }
    public SixLabors.ImageSharp.Image InternalImage { get; }
    //public PixelFormat Format { get; }

    private Image(SixLabors.ImageSharp.Image image, Bgra32[] palette = null)
    {
      InternalImage = image;
      Palette = palette;
    }

    public int Width => InternalImage.Width;
    public int Height => InternalImage.Height;
    
    // FIX(adm244): use original bytesPerPixel value (don't use InternalImage)
    public int BytesPerPixel => InternalImage.PixelType.BitsPerPixel / 8;

    public Image Convert(int bytesPerPixel)
    {
      PixelFormat format = GetPixelFormat(bytesPerPixel);
      return Convert(format);
    }

    public Image Convert(PixelFormat format)
    {
      switch (format)
      {
        case PixelFormat.Argb32:
          return new Image(InternalImage.CloneAs<Bgra32>());

        default:
          throw new NotSupportedException();
      }
    }

    public static Image LoadFromFile(string filepath)
    {
      return new Image(
        SixLabors.ImageSharp.Image.Load(filepath)
      );
    }

    public static Image FromImage(SixLabors.ImageSharp.Image image, Bgra32[] palette = null)
    {
      return new Image(image, palette);
    }

    public static Image FromBuffer(byte[] buffer, int width, int height, int bytesPerPixel, Bgra32[] palette)
    {
      // TODO(adm244): refactor this switch case
      switch (bytesPerPixel)
      {
        case 1:
          return new Image(
            ImageExtension.LoadIndexedAsBgra32(buffer, width, height, palette),
            palette
          );

        case 3:
          return new Image(
            // TODO(adm244): check if pixel format is correct
            ImageExtension.LoadPixelDataAsBgra32<Rgb24>(buffer, width, height)
          );

        case 4:
          return new Image(
            SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(buffer, width, height)
          );

        default:
          throw new NotSupportedException();
      }
    }

    public static PixelFormat GetPixelFormat(int bytesPerPixel)
    {
      switch (bytesPerPixel)
      {
        case 1:
          return PixelFormat.Indexed;
        case 2:
          return PixelFormat.Rgb565;
        case 3:
          return PixelFormat.Rgb24;
        case 4:
          return PixelFormat.Argb32;

        default:
          return PixelFormat.Undefined;
      }
    }
  }
}
