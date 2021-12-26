using System;
using System.IO;
using System.Text;
using AGSUnpackerSharp.Game;

namespace AGSUnpackerSharp.Utils
{
  public static class AGSIdentityExtractor
  {
    private static readonly string DTA_VERSION_3 = "game28.dta";
    private static readonly string DTA_VERSION_2 = "ac2game.dta";
    private static readonly string AGS_IDENTITY_FILENAME = "game_id.txt";

    public static bool ExtractIdentity(string filePath, string targetFolder)
    {
      Encoding encoding = Encoding.GetEncoding(1252);

      using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        using (BinaryReader r = new BinaryReader(fs, encoding))
        {
          Console.Write("Parsing {0}...", filePath);
          AGSAssetInfo[] assetInfos = AGSClibUtilsDeprecated.ParseAGSAssetInfos(r);
          Console.WriteLine(" Done!");

          AGSGameData dta = GetGameData(r, assetInfos);
          if (dta == null)
            return false;

          string targetPath = Path.Combine(targetFolder, AGS_IDENTITY_FILENAME);
          string content = string.Format("GameName: {0}\nUniqueID: {1}", dta.setup.name, dta.setup.unique_id);
          File.WriteAllText(targetPath, content, encoding);
        }
      }

      return true;
    }

    private static AGSGameData GetGameData(BinaryReader r, AGSAssetInfo[] assets)
    {
      if (assets == null)
        throw new InvalidDataException();

      for (int i = 0; i < assets.Length; ++i)
      {
        if ((assets[i].Filename == DTA_VERSION_2) || (assets[i].Filename == DTA_VERSION_3))
        {
          r.BaseStream.Seek(assets[i].Offset, SeekOrigin.Begin);

          byte[] buffer = r.ReadBytes((int)assets[i].Size);
          using (MemoryStream stream = new MemoryStream(buffer))
          {
            using (BinaryReader streamReader = new BinaryReader(stream, Encoding.GetEncoding(1252)))
            {
              AGSGameData dta = new AGSGameData();
              dta.LoadFromStream(streamReader);
              
              return dta;
            }
          }
        }
      }

      return null;
    }
  }
}
