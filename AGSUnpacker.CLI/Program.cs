using System;
using System.IO;
using System.IO.Compression;
using AGSUnpacker.Graphics;
using AGSUnpacker.Graphics.Formats;

namespace AGSUnpacker.CLI
{
  class Program
  {
    static void Main(string[] args)
    {
      // Bitmap bmp = new Bitmap(args[0]);
      // bmp.Save(args[1], ImageFormat.Png);

      using FileStream inputStream = new FileStream(args[0], FileMode.Open, FileAccess.Read);
      using FileStream outputStream = new FileStream(args[1], FileMode.Create, FileAccess.Write);

      // using MemoryStream scanlinesStream = new MemoryStream();

      {
        using ZLibStream zlibStream = new ZLibStream(outputStream, CompressionMode.Compress, leaveOpen: false);
        inputStream.CopyTo(zlibStream);
      }

      // TODO(adm244): read filter type and scanline
    }
  }
}
