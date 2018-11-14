using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AGSUnpackerSharp.Shared
{
  public class AGSInteractionScript
  {
    public AGSInteractionScriptEvent[] events;

    public AGSInteractionScript()
    {
      events = new AGSInteractionScriptEvent[0];
    }

    public void LoadFromStream(BinaryReader r)
    {
      Int32 events_count = r.ReadInt32();
      events = new AGSInteractionScriptEvent[events_count];
      for (int i = 0; i < events_count; ++i)
      {
        events[i].name = r.ReadNullTerminatedString(200);
      }
    }
  }
}
