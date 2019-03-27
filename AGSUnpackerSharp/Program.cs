using System;
using AGSUnpackerSharp.Graphics;
using AGSUnpackerSharp.Room;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace AGSUnpackerSharp
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length > 0)
      {
        string filepath = args[0];
        /*string[] files = AGSClibUtils.UnpackAGSAssetFiles(filepath);
        for (int i = 0; i < files.Length; ++i)
        {
          string filename = files[i].Substring(files[i].LastIndexOf('/') + 1);
          string extension = filename.Substring(filename.LastIndexOf('.') + 1);

          if (extension == "crm")
          {
            AGSRoom room = new AGSRoom();
            room.LoadFromFile(files[i]);
          }
        }*/
        //AGSSpritesCache.ExtractSprites(filepath);
        /*string[] files = Directory.GetFiles(filepath, "spr*");
        AGSSpritesCache.PackSprites(files);*/
        AGSRoom room = new AGSRoom();
        room.LoadFromFile(filepath);

        /*string[] files = Directory.GetFiles(filepath);
        Convert16bitTo32bitImages(files);*/
      }
      else
      {
        Console.WriteLine("ERROR: Filepath is not specified.");
      }
    }

    public static void Convert16bitTo32bitImages(params string[] files)
    {
      Color transparentColor = Color.FromArgb(255, 0, 255);
      for (int i = 0; i < files.Length; ++i)
      {
        Bitmap bitmap = new Bitmap(files[i]);
        bitmap.MakeTransparent(transparentColor);

        string filename = files[i].Split('.')[0];
        bitmap.Save(string.Format("{0}.png", filename), ImageFormat.Png);
      }
    }
  }
}
