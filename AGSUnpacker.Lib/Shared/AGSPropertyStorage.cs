using System;
using System.Diagnostics;
using System.IO;
using AGSUnpacker.Extensions;

namespace AGSUnpacker.Shared
{
  public class AGSPropertyStorage
  {
    public static readonly int MaxNameLength = 200;
    public static readonly int MaxValueLength = 500;

    public int Version;
    public string[] Names;
    public string[] Values;

    public AGSPropertyStorage()
    {
      Version = 2;
      Names = new string[0];
      Values = new string[0];
    }

    public void ReadFromStream(BinaryReader reader)
    {
      Version = reader.ReadInt32();
      Debug.Assert((Version == 1) || (Version == 2));

      int count = reader.ReadInt32();

      Names = new string[count];
      Values = new string[count];
      for (int i = 0; i < count; ++i)
      {
        if (Version == 1)
        {
          Names[i] = reader.ReadCString(MaxNameLength);
          Values[i] = reader.ReadCString(MaxValueLength);
        }
        else
        {
          Names[i] = reader.ReadPrefixedString32();
          Values[i] = reader.ReadPrefixedString32();
        }
      }
    }

    public void WriteToStream(BinaryWriter writer)
    {
      writer.Write((Int32)Version);

      Debug.Assert(Names.Length == Values.Length);
      writer.Write((Int32)Names.Length);

      for (int i = 0; i < Names.Length; ++i)
      {
        if (Version == 1)
        {
          writer.WriteCString(Names[i], MaxNameLength);
          writer.WriteCString(Values[i], MaxValueLength);
        }
        else
        {
          writer.WritePrefixedString32(Names[i]);
          writer.WritePrefixedString32(Values[i]);
        }
      }
    }
  }
}
