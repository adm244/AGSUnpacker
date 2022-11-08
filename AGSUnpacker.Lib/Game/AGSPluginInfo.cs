using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Game
{
  public class AGSPluginInfo
  {
    public string Name { get; set; }
    public byte[] Data { get; set; }

    public AGSPluginInfo()
    {
      Name = string.Empty;
      Data = Array.Empty<byte>();
    }

    public void ReadFromStream(BinaryReader reader, int version)
    {
      Name = reader.ReadCString();

      Int32 datasize = reader.ReadInt32();
      if (datasize > 0)
        Data = reader.ReadBytes(datasize);
    }

    public void WriteToStream(BinaryWriter writer, int version)
    {
      writer.WriteCString(Name);
      writer.Write((Int32)Data.Length);
      writer.Write((byte[])Data);
    }
  }
}
