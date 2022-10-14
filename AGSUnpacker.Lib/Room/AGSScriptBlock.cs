using System;
using System.IO;

namespace AGSUnpacker.Lib.Room
{
  public class AGSScriptBlock
  {
    const int EventsCount = 10;

    public AGSScriptEvent[] Events { get; private set; }
    public int Count { get; private set; }

    private AGSScriptBlock()
    {
    }

    static public AGSScriptBlock ReadFromStream(BinaryReader reader)
    {
      AGSScriptBlock scriptBlock = new AGSScriptBlock();

      int count = reader.ReadInt32();
      if (count < 0 || count > EventsCount)
        throw new InvalidDataException($"Invalid graphical script events count: {count:X}");

      scriptBlock.Count = count;

      scriptBlock.Events = new AGSScriptEvent[EventsCount];
      for (int i = 0; i < scriptBlock.Events.Length; ++i)
        scriptBlock.Events[i] = AGSScriptEvent.ReadFromStream(reader);

      return scriptBlock;
    }

    public void WriteToStream(BinaryWriter writer)
    {
      writer.Write((Int32)Count);
      
      for (int i = 0; i < Events.Length; ++i)
        Events[i].WriteToStream(writer);
    }
  }
}
