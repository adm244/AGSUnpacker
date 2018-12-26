using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace AGSUnpackerSharp.Translation
{
  public class AGSTranslation
  {
    public List<string> OriginalLines = new List<string>();
    public List<string> TranslatedLines = new List<string>();

    //TODO(adm244): implement *.tra compile

    public void Decompile(string filepath)
    {
      Int32 unique_id = 0;
      string game_name = string.Empty;

      FileStream fs = new FileStream(filepath, FileMode.Open);
      BinaryReader r = new BinaryReader(fs, Encoding.GetEncoding(1252));

      r.BaseStream.Seek(15, SeekOrigin.Begin);

      for (; ; )
      {
        Int32 blockType = r.ReadInt32();
        Int32 blockSize = r.ReadInt32();

        if (blockType == 0x1)
        {
          for (; ; )
          {
            string original = AGSStringUtils.ReadEncryptedString(r);
            string translation = AGSStringUtils.ReadEncryptedString(r);
            OriginalLines.Add(original);
            TranslatedLines.Add(translation);

            if ((original.Length < 1) && (translation.Length < 1)) break;
          }
        }
        else if (blockType == 0x2)
        {
          unique_id = r.ReadInt32();
          game_name = AGSStringUtils.ReadEncryptedString(r);
        }
        else if (blockType == 0x3)
        {
          //TODO(adm244): read config section
          break;
        }
        else
        {
          Debug.Assert(false, "Unknown block type encountered!");
        }
      }

      r.Close();
    }
  }
}
