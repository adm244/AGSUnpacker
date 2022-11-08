using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Game
{
  public class AGSCustomProperty
  {
    public string Name;
    public string Description;
    public string DefaultValue;
    public int Type;

    public AGSCustomProperty()
    {
    }

    public void ReadFromStream(BinaryReader reader, int version)
    {
      if (version == 1)
      {
        Name = reader.ReadCString(20);
        Description = reader.ReadCString(100);
        DefaultValue = reader.ReadCString(500);
        Type = reader.ReadInt32();
      }
      else
      {
        Name = reader.ReadPrefixedString32();
        Type = reader.ReadInt32();
        Description = reader.ReadPrefixedString32();
        DefaultValue = reader.ReadPrefixedString32();
      }
    }

    public void WriteToStream(BinaryWriter writer, int version)
    {
      if (version == 1)
      {
        writer.WriteCString(Name, 20);
        writer.WriteCString(Description, 100);
        writer.WriteCString(DefaultValue, 500);
        writer.Write((Int32)Type);
      }
      else
      {
        writer.WritePrefixedString32(Name);
        writer.Write((Int32)Type);
        writer.WritePrefixedString32(Description);
        writer.WritePrefixedString32(DefaultValue);
      }
    }
  }
}
