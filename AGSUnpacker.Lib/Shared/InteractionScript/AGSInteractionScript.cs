using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Shared
{
  public class AGSInteractionScript
  {
    public int version;
    public string scriptModule;
    public AGSInteractionScriptEvent[] events;

    public AGSInteractionScript()
    {
      version = 0;
      scriptModule = string.Empty;
      events = new AGSInteractionScriptEvent[0];
    }

    public void LoadFromStream(BinaryReader r)
    {
      Int32 events_count = r.ReadInt32();
      events = new AGSInteractionScriptEvent[events_count];
      for (int i = 0; i < events_count; ++i)
      {
        events[i].name = r.ReadCString();
      }
    }

    public void WriteToStream(BinaryWriter w)
    {
      w.Write((Int32)events.Length);
      for (int i = 0; i < events.Length; ++i)
      {
        w.WriteCString(events[i].name);
      }
    }

    public void LoadFromStream_v362(BinaryReader r)
    {
      version = r.ReadInt32();
      if (version != (int)Versions.v362)
        throw new NotSupportedException($"Unknown interaction script version: {version}");

      scriptModule = r.ReadPrefixedString32();

      int count = r.ReadInt32();
      events = new AGSInteractionScriptEvent[count];
      for (int i = 0; i < count; ++i)
      {
        events[i].name = r.ReadPrefixedString32();
      }
    }

    public void WriteToStream_v362(BinaryWriter w)
    {
      w.Write((Int32)version);
      w.WritePrefixedString32(scriptModule);

      w.Write((Int32)events.Length);
      for (int i = 0; i < events.Length; ++i)
      {
        w.WritePrefixedString32(events[i].name);
      }
    }

    public enum Versions
    {
      v362 = 3060200
    }
  }
}
