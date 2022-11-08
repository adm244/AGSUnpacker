using System.IO;

using AGSUnpacker.Lib.Assets;
using AGSUnpacker.Lib.Game;
using AGSUnpacker.Lib.Graphics;
using AGSUnpacker.Lib.Room;
using AGSUnpacker.Lib.Translation;

namespace AGSUnpacker.CLI
{
  class Program
  {
    static void Main(string[] args)
    {
      //AssetsManager manager = AssetsManager.Create(args[0]);
      //manager.Extract(args[1]);

      //AGSSpriteSet.UnpackSprites(args[0], args[1]);

      //AGSTranslation translation = AGSTranslation.ReadSourceFile(args[0]);
      //translation.Compile(args[1], 1302728765, "A Golden Wake");

      //SaveRoomFrames(args[0]);

      SetDebugMode(args[0], true);
    }

    private static void SetDebugMode(string filepath, bool value)
    {
      AGSGameData data = new AGSGameData();
      data.LoadFromFile(filepath);

      //data.setup.options[0] = value ? 1 : 0;

      //data.SaveToFile(filepath);
    }

    private static void SaveRoomFrames(string sourceFolder)
    {
      string[] files = Directory.GetFiles(sourceFolder);
      for (int i = 0; i < files.Length; ++i)
      {
        if (Path.GetExtension(files[i]) == ".crm")
        {
          AGSRoom room = new AGSRoom();
          room.ReadFromFileDeprecated(files[i]);

          string directory = Path.GetDirectoryName(files[i]);
          string filename = Path.GetFileNameWithoutExtension(files[i]);

          string baseFilepath = Path.Combine(directory, filename);
          for (int k = 0; k < room.Background.Frames.Count; ++k)
          {
            string filepath = $"{baseFilepath}_frame{k}";
            room.Background.Frames[k].Save(filepath, Graphics.Formats.ImageFormat.Png);
          }
        }
      }
    }
  }
}
