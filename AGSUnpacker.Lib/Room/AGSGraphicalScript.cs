using System;
using System.IO;

namespace AGSUnpacker.Lib.Room
{
  public class AGSGraphicalScript
  {
    private const int BlockVersion = 2;
    private const int BlockSize = 0xFE;

    public int Id { get; private set; }
    public AGSScriptBlock[] Blocks { get; private set; }

    private AGSGraphicalScript()
    {
    }

    public static AGSGraphicalScript ReadFromStream(BinaryReader reader, int id)
    {
      AGSGraphicalScript script = new AGSGraphicalScript();
      script.Id = id;

      int version = reader.ReadInt32();

      // NOTE(adm244): version 1 is pre 2.00 which is out of scope of this project
      if (version != BlockVersion)
        throw new NotSupportedException($"Unknown graphical script version: {version}");

      int blockSize = reader.ReadInt32();

      // NOTE(adm244): version 2 has only this variant
      if (blockSize != BlockSize)
        throw new NotSupportedException($"Unknown graphical script block size: {blockSize}");

      int blocksCount = reader.ReadInt32();
      if (blocksCount < 0)
        throw new InvalidDataException($"Invalid graphical script blocks count: {blocksCount:X}");

      script.Blocks = new AGSScriptBlock[blocksCount];
      for (int i = 0; i < script.Blocks.Length; ++i)
        script.Blocks[i] = AGSScriptBlock.ReadFromStream(reader);

      return script;
    }

    public void WriteToStream(BinaryWriter writer)
    {
      writer.Write((Int32)BlockVersion);
      writer.Write((Int32)BlockSize);
      writer.Write((Int32)Blocks.Length);

      for (int i = 0; i < Blocks.Length; ++i)
        Blocks[i].WriteToStream(writer);
    }
  }
}
