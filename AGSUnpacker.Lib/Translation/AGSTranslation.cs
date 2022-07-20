using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Translation
{
  public class AGSTranslation
  {
    private static readonly string TRA_SIGNATURE = "AGSTranslation\x0";

    public static readonly string TRS_TAG_GAMEID = "//#GameId=";
    public static readonly string TRS_TAG_GAMENAME = "//#GameName=";

    // FIXME(adm244): temporary? public
    public List<string> OriginalLines { get; set; }
    public List<string> TranslatedLines { get; set; }

    public int GameID { get; private set; }
    public string GameName { get; private set; }

    public Dictionary<string, string> Options;

    public AGSTranslation()
    {
      OriginalLines = new List<string>();
      TranslatedLines = new List<string>();

      GameID = 0;
      GameName = string.Empty;

      Options = new Dictionary<string, string>();
    }

    public AGSTranslation(IEnumerable<string> originalLines, IEnumerable<string> translatedLines)
    {
      OriginalLines.AddRange(originalLines);
      TranslatedLines.AddRange(translatedLines);
    }

    public bool Add(string original, string translation)
    {
      if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(translation))
        return false;

      OriginalLines.Add(original);
      TranslatedLines.Add(translation);

      return true;
    }

    public void Compile(string filepath)
    {
      Compile(filepath, GameID, GameName);
    }

    public void Compile(string filepath, int gameID, string gameName)
    {
      if (OriginalLines.Count != TranslatedLines.Count)
      {
        Trace.Assert(false, "AGSTranslation::Compile: Original and Tranlated lines count do not match!");
        return;
      }

      GameID = gameID;
      GameName = gameName;

      using (FileStream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
      {
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Latin1))
        {
          writer.WriteFixedString(TRA_SIGNATURE, TRA_SIGNATURE.Length);

          WriteBlock(writer, BlockType.Header);
          WriteBlock(writer, BlockType.Content);

          //TODO(adm244): implement settings block
          //WriteBlock(writer, BlockType.Settings);

          WriteBlock(writer, BlockType.End);
        }
      }
    }

    private void WriteBlock(BinaryWriter writer, BlockType type)
    {
      writer.Write((UInt32)type);
      writer.Write((UInt32)0xDEADBEAF);

      long blockStart = writer.BaseStream.Position;

      switch (type)
      {
        case BlockType.Header:
          {
            writer.Write((UInt32)GameID);
            writer.WriteEncryptedCString(GameName);
          } break;

        case BlockType.Content:
          {
            for (int i = 0; i < OriginalLines.Count; ++i)
            {
              //NOTE(adm244): skip lines that have no translation
              if (string.IsNullOrEmpty(TranslatedLines[i]))
                continue;

              writer.WriteEncryptedCString(OriginalLines[i]);
              writer.WriteEncryptedCString(TranslatedLines[i]);
            }

            //NOTE(adm244): write empty strings so parser knows this block has ended
            // no idea why they just didn't use a block size value...
            writer.WriteEncryptedCString("");
            writer.WriteEncryptedCString("");
          } break;

        case BlockType.Settings:
          throw new NotImplementedException("AGSTranslation: Setting block is not implemented!");

        case BlockType.End:
          // do nothing
          break;

        default:
          throw new NotImplementedException("AGSTranslation: Block type is not implemented!");
      }

      long blockEnd = writer.BaseStream.Position;

      //NOTE(adm244): go back and write a block size
      UInt32 blockSize = (UInt32)(blockEnd - blockStart);
      writer.BaseStream.Position = (blockStart - sizeof(UInt32));
      writer.Write((UInt32)blockSize);

      writer.BaseStream.Position = blockEnd;
    }

    public void Decompile(string filepath)
    {
      using (FileStream stream = new FileStream(filepath, FileMode.Open))
      {
        using (BinaryReader reader = new BinaryReader(stream, Encoding.Latin1))
        {
          string signature = reader.ReadFixedString(15);
          if (signature != TRA_SIGNATURE)
          {
            Debug.Assert(false, "Invalid TRA signature!");
            return;
          }

          for (; ; )
          {
            Int32 blockType = reader.ReadInt32();

            if (blockType == (int)BlockType.End)
              break;

            if (blockType == (int)BlockType.Extension)
            {
              string id = reader.ReadFixedCString(16);

              //NOTE(adm244): there are two places in ags source that supposedly
              // write translation files: cpp code and csharp code;
              // "cpp" writes size as 32-bit integer and has a bug (yep) in calculation
              // that makes it 4-bytes short, but luckily is never called from anywhere;
              // "csharp" writes size as 64-bit integer, hardcodes all values and
              // actually is being used for writing tra files.
              //
              // Thus, we read 64-bit integer here (and are not using ExtensionBlock)
              long size = reader.ReadInt64();

              long blockEnd = reader.BaseStream.Position + size;
              if (!ReadExtensionBlock(reader, id, size))
              {
                //NOTE(adm244): skip extension block if it cannot be parsed
                Debug.Assert(false, $"Cannot read translation extension block '{blockType}'!");
                reader.BaseStream.Seek(blockEnd, SeekOrigin.Begin);
              }
            }
            else
            {
              long size = reader.ReadInt32();

              long blockEnd = reader.BaseStream.Position + size;
              if (!ReadTranslationBlock(reader, (BlockType)blockType, size))
              {
                //NOTE(adm244): skip block if it cannot be parsed
                Debug.Assert(false, $"Cannot read translation block '{blockType}'!");
                reader.BaseStream.Seek(blockEnd, SeekOrigin.Begin);
              }
            }
          }
        }
      }
    }

    //FIXME(adm244): duplicate of AGSRoom.ReadOptionsExtensionBlock
    private bool ReadOptionsExtensionBlock(BinaryReader reader)
    {
      int count = reader.ReadInt32();

      for (int i = 0; i < count; ++i)
      {
        string key = reader.ReadPrefixedString32();
        string value = reader.ReadPrefixedString32();

        Options.Add(key, value);
      }

      return true;
    }

    private bool ReadExtensionBlock(BinaryReader reader, string id, long size)
    {
      switch (id)
      {
        case "ext_sopts":
          return ReadOptionsExtensionBlock(reader);

        default:
          Debug.Assert(false, $"Unknown extension block '{id}' encountered!");
          return false;
      }
    }

    private bool ReadTranslationBlock(BinaryReader reader, BlockType type, long size)
    {
      //TODO(adm244): check size

      switch (type)
      {
        case BlockType.Content:
        {
          for (; ; )
          {
            string original = reader.ReadEncryptedCString();
            string translation = reader.ReadEncryptedCString();

            if ((original.Length < 1) && (translation.Length < 1))
              return true;

            OriginalLines.Add(original);
            TranslatedLines.Add(translation);
          }
        }

        case BlockType.Header:
          GameID = reader.ReadInt32();
          //FIXME(adm244): unicode support !!!
          GameName = reader.ReadEncryptedCString();
          return true;

        case BlockType.Settings:
          //TODO(adm244): implement settings block
          return false;

        default:
          Debug.Assert(false, "Unknown block type encountered!");
          return false;
      }
    }

    public static AGSTranslation ReadSourceFile(string filename)
    {
      AGSTranslation translation = new AGSTranslation();

      using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
      {
        using (StreamReader reader = new StreamReader(stream, Encoding.Latin1))
        {
          ReadState state = ReadState.Original;

          while (!reader.EndOfStream)
          {
            string line = reader.ReadLine();
            if (line.StartsWith("//"))
            {
              if (line.StartsWith(TRS_TAG_GAMEID))
              {
                string gameIDString = line.Substring(TRS_TAG_GAMEID.Length);
                translation.GameID = int.Parse(gameIDString);
              }
              else if (line.StartsWith(TRS_TAG_GAMENAME))
              {
                translation.GameName = line.Substring(TRS_TAG_GAMENAME.Length);
              }

              continue;
            }

            //TODO(adm244): read settings

            switch (state)
            {
              case ReadState.Original:
                {
                  translation.OriginalLines.Add(line);
                  state = ReadState.Translation;
                }
                break;

              case ReadState.Translation:
                {
                  translation.TranslatedLines.Add(line);
                  state = ReadState.Original;
                }
                break;

              default:
                throw new InvalidDataException();
            }
          }
        }
      }

      return translation;
    }

    public void WriteSourceFile(string filepath)
    {
      using (FileStream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
      {
        using (StreamWriter writer = new StreamWriter(stream, Encoding.Latin1))
        {
          Debug.Assert(OriginalLines.Count == TranslatedLines.Count);

          // TODO(adm244): assert GameID and GameName are valid

          writer.WriteLine("{0}{1}", TRS_TAG_GAMEID, GameID);
          writer.WriteLine("{0}{1}", TRS_TAG_GAMENAME, GameName);

          for (int i = 0; i < OriginalLines.Count; ++i)
          {
            writer.WriteLine(OriginalLines[i]);
            writer.WriteLine(TranslatedLines[i]);
          }
        }
      }
    }

    private enum BlockType
    {
      End = -1,
      Extension = 0,
      Content = 1,
      Header = 2,
      Settings = 3,
    }

    private enum ReadState
    {
      Invalid = 0,
      Original,
      Translation,
    }
  }
}
