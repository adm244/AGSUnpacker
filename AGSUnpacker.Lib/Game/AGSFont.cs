using System;
using System.IO;

namespace AGSUnpacker.Lib.Game
{
  public class AGSFont
  {
    public int Flags { get; set; }
    public int Outline { get; set; }

    public int OffsetY { get; set; }
    public int LineSpacing { get; set; }
    public int SizePt { get; set; }

    public AGSFont()
    {
      Flags = 0;
      Outline = 0;

      OffsetY = 0;
      LineSpacing = 0;
      SizePt = 0;
    }

    public void ReadFromStream(BinaryReader reader)
    {
      Flags = reader.ReadInt32();
      SizePt = reader.ReadInt32();
      Outline = reader.ReadInt32();
      OffsetY = reader.ReadInt32();
      LineSpacing = reader.ReadInt32();
    }

    public void WriteToStream(BinaryWriter writer)
    {
      writer.Write((Int32)Flags);
      writer.Write((Int32)SizePt);
      writer.Write((Int32)Outline);
      writer.Write((Int32)OffsetY);
      writer.Write((Int32)LineSpacing);
    }
  }
}
