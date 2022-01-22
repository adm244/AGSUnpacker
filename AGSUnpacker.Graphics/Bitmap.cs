using System.IO;

using AGSUnpacker.Graphics.Formats;

namespace AGSUnpacker.Graphics
{
  public class Bitmap
  {
    public PixelFormat Format => Instance.Format;
    public Palette Palette => Instance.Palette;

    public int Width => Instance.Width;
    public int Height => Instance.Height;
    public int BytesPerPixel => Format.GetBytesPerPixel();

    private BitmapInstance Instance { get; }

    public Bitmap(string filepath)
    {
      Instance = BitmapInstance.Create(filepath);
    }

    public Bitmap(int width, int height, byte[] buffer, PixelFormat format, Palette? palette = null)
    {
      Instance = BitmapInstance.Create(width, height, buffer, format, palette);
    }

    private Bitmap(BitmapInstance instance)
    {
      Instance = instance;
    }

    public Bitmap Convert(PixelFormat format)
    {
      BitmapInstance instance = Instance.Convert(format);
      return new Bitmap(instance);
    }

    public byte[] GetPixels()
    {
      return Instance.GetPixels();
    }

    public void Save(string filepath, ImageFormat format)
    {
      string directory = Path.GetDirectoryName(filepath);
      // NOTE(adm244): keep the extension if any
      string filename = Path.GetFileName(filepath);
      string extension = format.GetExtension();

      filepath = Path.Combine(directory, filename + extension);
      Instance.Save(filepath, format);
    }
  }
}
