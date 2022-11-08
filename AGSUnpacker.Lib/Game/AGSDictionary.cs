using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Game
{
  public struct AGSDictionaryWord
  {
    public string text;
    public Int16 group;
  }

  public class AGSDictionary
  {
    public AGSDictionaryWord[] words;

    public AGSDictionary()
    {
      words = new AGSDictionaryWord[0];
    }

    public void LoadFromStream(BinaryReader reader)
    {
      Int32 words_count = reader.ReadInt32();
      words = new AGSDictionaryWord[words_count];
      for (int i = 0; i < words_count; ++i)
      {
        words[i].text = reader.ReadEncryptedCString();
        words[i].group = reader.ReadInt16();
      }
    }

    public void WriteToStream(BinaryWriter writer)
    {
      writer.Write((Int32)words.Length);

      for (int i = 0; i < words.Length; ++i)
      {
        writer.WriteEncryptedCString(words[i].text);
        writer.Write((Int16)words[i].group);
      }
    }
  }
}
