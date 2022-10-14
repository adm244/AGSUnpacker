using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Room
{
  public class AGSEventBlock
  {
    public const int CommandsMax = 8;

    public int[] Lists { get; private set; }
    public int[] Responds { get; private set; }
    public int[] Values { get; private set; }
    public int[] Data { get; private set; }
    public short[] Scores { get; private set; }

    public int Count { get; private set; }

    private AGSEventBlock()
    {
      Lists = Array.Empty<int>();
      Responds = Array.Empty<int>();
      Values = Array.Empty<int>();
      Data = Array.Empty<int>();
      Scores = Array.Empty<short>();
      
      Count = 0;
    }

    public static AGSEventBlock ReadFromStream(BinaryReader reader)
    {
      AGSEventBlock block = new AGSEventBlock();

      block.Lists = reader.ReadArrayInt32(CommandsMax);
      block.Responds = reader.ReadArrayInt32(CommandsMax);
      block.Values = reader.ReadArrayInt32(CommandsMax);
      block.Data = reader.ReadArrayInt32(CommandsMax);

      block.Count = reader.ReadInt32();

      block.Scores = reader.ReadArrayInt16(CommandsMax);

      return block;
    }

    public void WriteToStream(BinaryWriter writer)
    {
      writer.WriteArrayInt32(Lists);
      writer.WriteArrayInt32(Responds);
      writer.WriteArrayInt32(Values);
      writer.WriteArrayInt32(Data);

      writer.Write((Int32)Count);

      writer.WriteArrayInt16(Scores);
    }
  }
}
