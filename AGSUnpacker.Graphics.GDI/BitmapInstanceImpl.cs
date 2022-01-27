using System;
using System.IO;
using System.Runtime.Versioning;

using AGSUnpacker.Graphics.Formats;
using AGSUnpacker.Graphics.GDI.Extensions;

[assembly: SupportedOSPlatform("windows")]
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
      System.Drawing.Imaging.PixelFormat gdiFormat = format.ToGDIFormat();

      Instance = new System.Drawing.Bitmap(width, height, gdiFormat);
      Instance.SetPixels(buffer);

      if (palette.HasValue)
        Instance.SetPalette(palette.Value);
    }

    private BitmapInstanceImpl(System.Drawing.Bitmap bitmap)
    {
      BitmapInstanceInfo bitmapInfo = new BitmapInstanceInfo
      {
        Width = bitmap.Width,
        Height = bitmap.Height,
        Buffer = bitmap.GetPixels(),
        Format = bitmap.PixelFormat.ToAGSFormat(),
        Palette = bitmap.Palette.ToAGSPalette()
      };

      Initialize(bitmapInfo);
    }

    internal System.Drawing.Bitmap Instance { get; private set; }

    public override BitmapInstance Convert(PixelFormat format)
    {
      System.Drawing.Imaging.PixelFormat gdiFormat = format.ToGDIFormat();
      System.Drawing.Bitmap bitmap = Instance.Convert(gdiFormat);

      return new BitmapInstanceImpl(bitmap);
    }

    public override byte[] GetPixels()
    {
      return Instance.GetPixels();
    }

    public override void Save(string filepath, ImageFormat format)
    {
      switch (format)
      {
        case ImageFormat.Png:
          this.SaveAsPng(filepath);
          break;

        default:
          throw new NotSupportedException();
      }
    }

    protected override BitmapInstanceInfo LoadFromFile(string filepath)
    {
      //RANT(adm244): thanks to Bitmap's busted constructor that does not close a file in time
      // (and apparently doesn't allow reading), we are forced to handle it ourselves.
      using (FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
        Instance = new System.Drawing.Bitmap(stream);

      if (Instance == null)
        throw new InvalidDataException();

      //NOTE(adm244): GDI+ decides for some reason that 16-bits is 32-bits when loading bmp (a bug?),
      // so we have to parse bits count field manually...
      //TODO(adm244): double check that
      if (Path.GetExtension(filepath) == ".bmp")
      {
        System.Drawing.Imaging.PixelFormat format = BitmapInstanceImplExtension.ReadBitmapPixelFormat(filepath);
        Instance = Instance.Convert(format);
      }

      return new BitmapInstanceInfo
      {
        Width = Instance.Width,
        Height = Instance.Height,
        Palette = Instance.Palette.Entries.Length > 0 ? Instance.Palette.ToAGSPalette() : null,
        Format = Instance.PixelFormat.ToAGSFormat(),
        Buffer = Instance.GetPixels()
      };
    }
  }
}
