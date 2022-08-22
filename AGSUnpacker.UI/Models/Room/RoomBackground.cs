using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using AGSUnpacker.Lib.Room;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AGSUnpacker.UI.Models.Room
{
  internal class RoomBackground
  {
    public AGSRoomBackground Base { get; }
    public IList<RoomFrame> Frames { get; }

    public RoomBackground(AGSRoomBackground roomBackground)
    {
      Base = roomBackground;
      
      IList<RoomFrame> frames = new List<RoomFrame>(roomBackground.Frames.Count);
      for (int i = 0; i < roomBackground.Frames.Count; ++i)
      {
        string name = i == 0 ? "Background" : $"Frame {i}";
        frames.Add(new RoomFrame(roomBackground.Frames[i], name));
      }

      Frames = frames;
    }

    public void ChangeFrame(int index, Graphics.Bitmap bitmap)
    {
      Frames[index].ChangeImage(bitmap);
    }
  }

  internal class RoomFrame : ObservableObject
  {
    public string Name { get; }

    private BitmapSource _source;
    public BitmapSource Source
    {
      get => _source;
      private set => SetProperty(ref _source, value);
    }

    public RoomFrame(Graphics.Bitmap bitmap, string name)
    {
      Name = name;
      Source = bitmap.ToWpf();
    }

    public void ChangeImage(Graphics.Bitmap bitmap)
    {
      Source = bitmap.ToWpf();
    }
  }

  internal static partial class BitmapExtensions
  {
    public static BitmapSource ToWpf(this Graphics.Bitmap bitmap)
    {
      // HACK(adm244): get rid of transparent alpha channel
      //if (bitmap.Format == Graphics.Formats.PixelFormat.Argb32)
      //  bitmap = bitmap.Convert(Graphics.Formats.PixelFormat.Rgb24);

      byte[] buffer = bitmap.GetPixels();
      int stride = bitmap.Width * bitmap.BytesPerPixel;
      return BitmapSource.Create(bitmap.Width, bitmap.Height, 96, 96,
        bitmap.Format.ToWpf(), bitmap.Palette?.ToWpf(), buffer, stride);
    }
  }

  internal static partial class PixelFormatExtensions
  {
    public static PixelFormat ToWpf(this Graphics.Formats.PixelFormat format)
    {
      switch (format)
      {
        case Graphics.Formats.PixelFormat.Indexed:
          return PixelFormats.Indexed8;

        case Graphics.Formats.PixelFormat.Rgb565:
          return PixelFormats.Bgr565;

        case Graphics.Formats.PixelFormat.Rgb24:
          return PixelFormats.Bgr24;

        case Graphics.Formats.PixelFormat.Argb32:
          return PixelFormats.Bgra32;

        default:
          throw new NotSupportedException();
      }
    }
  }

  internal static class PaletteExtension
  {
    public static BitmapPalette ToWpf(this Graphics.Palette palette)
    {
      IList<Color> colors = new List<Color>(palette.Length);
      
      for (int i = 0; i < palette.Length; ++i)
      {
        Color color = Color.FromArgb(palette[i].A, palette[i].R, palette[i].G, palette[i].B);
        colors.Add(color);
      }

      return new BitmapPalette(colors);
    }
  }
}
