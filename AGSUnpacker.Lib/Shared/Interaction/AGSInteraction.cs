using System;
using System.Diagnostics;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Shared.Interaction
{
  public class AGSInteraction
  {
    public AGSInteractionCommandsList[] events;
    public Int32 version;
    public Int32[] event_responses;

    public AGSInteraction()
    {
      events = new AGSInteractionCommandsList[0];
      version = 1;
    }

    public void LoadFromStream(BinaryReader r)
    {
      Int32 version = r.ReadInt32();
      Debug.Assert(version == 1);

      Int32 events_count = r.ReadInt32();
      events = new AGSInteractionCommandsList[events_count];

      Int32[] types = new Int32[events.Length];
      // NOTE(adm244): read as ArrayInt32
      for (int i = 0; i < events.Length; ++i)
        types[i] = r.ReadInt32();

      event_responses = r.ReadArrayInt32(events.Length);
      for (int i = 0; i < event_responses.Length; ++i)
      {
        events[i] = null;

        if (event_responses[i] == 0)
          continue;

        events[i] = new AGSInteractionCommandsList();
        events[i].LoadFromStream(r);
        events[i].type = types[i];
      }
    }

    public void WriteToStream(BinaryWriter w)
    {
      w.Write((Int32)version);
      w.Write((Int32)events.Length);

      for (int i = 0; i < events.Length; ++i)
      {
        if (events[i] == null)
          w.Write((Int32)0x0);
        else
          w.Write((Int32)events[i].type);
      }

      for (int i = 0; i < event_responses.Length; ++i)
        w.Write((Int32)event_responses[i]);

      for (int i = 0; i < events.Length; ++i)
      {
        if (events[i] == null)
          continue;

        events[i].WriteToStream(w);
      }
    }
  }
}
