using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AGSUnpackerSharp.Game
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
  }
}
