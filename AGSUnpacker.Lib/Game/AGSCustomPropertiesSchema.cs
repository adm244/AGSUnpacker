using System;
using System.IO;

namespace AGSUnpacker.Lib.Game
{
  public class AGSCustomPropertiesSchema
  {
    public int Version;
    public AGSCustomProperty[] Properties;

    public AGSCustomPropertiesSchema()
    {
      Version = 0;
      Properties = Array.Empty<AGSCustomProperty>();
    }

    public void LoadFromStream(BinaryReader r)
    {
      Version = r.ReadInt32();
      if ((Version != 1) && (Version != 2))
        throw new NotSupportedException($"Unsupported custom properties format version: {Version}.");

      Int32 count = r.ReadInt32();
      Properties = new AGSCustomProperty[count];
      for (int i = 0; i < count; ++i)
      {
        Properties[i] = new AGSCustomProperty();
        Properties[i].ReadFromStream(r, Version);
      }
    }

    public void WriteToStream(BinaryWriter writer)
    {
      writer.Write((Int32)Version);

      writer.Write((Int32)Properties.Length);
      for (int i = 0; i < Properties.Length; ++i)
        Properties[i].WriteToStream(writer, Version);
    }
  }
}
