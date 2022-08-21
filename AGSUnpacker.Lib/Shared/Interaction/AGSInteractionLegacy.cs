using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Shared.Interaction
{
  public class AGSInteractionLegacy
  {
    public string Name;
    public byte Type;
    public int Value;

    public AGSInteractionLegacy()
    {
      Name = string.Empty;
      Type = 0;
      Value = 0;
    }

    public void ReadFromStream(BinaryReader reader)
    {
      Name = reader.ReadFixedCString(23);
      Type = reader.ReadByte();
      Value = reader.ReadInt32();
    }

    public void WriteToStream(BinaryWriter writer)
    {
      writer.WriteFixedString(Name, 23);
      writer.Write((byte)Type);
      writer.Write((Int32)Value);
    }
  }
}
