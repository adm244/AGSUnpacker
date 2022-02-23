using System;
using System.IO;
using System.Text;

using AGSUnpacker.Lib.Game;

namespace AGSUnpacker.Lib.Utils
{
  public static class AGSIdentityExtractor
  {
    //private static readonly string DTA_VERSION_3 = "game28.dta";
    //private static readonly string DTA_VERSION_2 = "ac2game.dta";
    //private static readonly string AGS_IDENTITY_FILENAME = "game_id.txt";

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

    //public static bool ExtractIdentity(string filePath, string targetFolder)
    //{
    //  Encoding encoding = Encoding.Latin1;
    //
    //  using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
    //  {
    //    using (BinaryReader r = new BinaryReader(fs, encoding))
    //    {
    //      Console.Write("Parsing {0}...", filePath);
    //      AGSAssetInfo[] assetInfos = AGSClibUtilsDeprecated.ParseAGSAssetInfos(r);
    //      Console.WriteLine(" Done!");
    //
    //      AGSGameData dta = GetGameData(r, assetInfos);
    //      if (dta == null)
    //        return false;
    //
    //      string targetPath = Path.Combine(targetFolder, AGS_IDENTITY_FILENAME);
    //      string content = string.Format("GameName: {0}\nUniqueID: {1}", dta.setup.name, dta.setup.unique_id);
    //      File.WriteAllText(targetPath, content, encoding);
    //    }
    //  }
    //
    //  return true;
    //}
    //
    //// FIXME(adm244): code duplication; see TextExtractor
    //private static AGSGameData GetGameData(BinaryReader r, AGSAssetInfo[] assets)
    //{
    //  if (assets == null)
    //    throw new InvalidDataException();
    //
    //  for (int i = 0; i < assets.Length; ++i)
    //  {
    //    if ((assets[i].Filename == DTA_VERSION_2) || (assets[i].Filename == DTA_VERSION_3))
    //    {
    //      r.BaseStream.Seek(assets[i].Offset, SeekOrigin.Begin);
    //
    //      byte[] buffer = r.ReadBytes((int)assets[i].Size);
    //      using (MemoryStream stream = new MemoryStream(buffer))
    //      {
    //        using (BinaryReader streamReader = new BinaryReader(stream, Encoding.Latin1))
    //        {
    //          AGSGameData dta = new AGSGameData();
    //          dta.LoadFromStream(streamReader);
    //          
    //          return dta;
    //        }
    //      }
    //    }
    //  }
    //
    //  return null;
    //}
  }
}
