using System;
using System.IO;
using System.Text;

using AGSUnpacker.Lib.Game;

namespace AGSUnpacker.Lib.Utils
{
  public static class AGSIdentityExtractor
  {
    // TODO(adm244): rewrite using AssetsManager to support multilib files
    public static bool ExtractFromFolder(string sourceFolder, string targetFile)
    {
      if (Directory.Exists(sourceFolder))
      {
        AGSGameData gameData = new AGSGameData();
        string[] filenames = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);

        Console.WriteLine("Searching asset files...");

        for (int i = 0; i < filenames.Length; ++i)
        {
          int index = filenames[i].LastIndexOf('.');
          string fileExtension = filenames[i].Substring(index + 1);

          if (fileExtension == "dta")
          {
            Console.Write("\tParsing {0} data file...", Path.GetFileName(filenames[i]));
            gameData.LoadFromFile(filenames[i]);
            Console.WriteLine(" Done!");

            break;
          }
        }

        Console.Write("Extracting game id...");

        string content = string.Format("GameName: {0}\nUniqueID: {1}",
          gameData.setup.name, gameData.setup.unique_id);
        File.WriteAllText(targetFile, content, Encoding.Latin1);

        Console.WriteLine(" Done!");

        return true;
      }
      else
      {
        Console.WriteLine(string.Format("ERROR: Folder {0} does not exist.", sourceFolder));
        return false;
      }
    }
  }
}
