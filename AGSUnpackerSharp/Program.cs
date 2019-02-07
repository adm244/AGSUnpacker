using System;
using AGSUnpackerSharp.Graphics;
using AGSUnpackerSharp.Room;
using System.IO;

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
      }
      else
      {
        Console.WriteLine("ERROR: Filepath is not specified.");
      }
    }
  }
}
