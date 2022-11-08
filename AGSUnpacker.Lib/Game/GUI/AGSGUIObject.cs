using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Game
{
  public class AGSGUIObject
  {
    public Int32 flags;
    public Int32 x;
    public Int32 y;
    public Int32 width;
    public Int32 height;
    public Int32 z_order;
    public Int32 is_activated;
    public string name;
    public string[] events;

    public AGSGUIObject()
    {
      flags = 0;
      x = 0;
      y = 0;
      width = 0;
      height = 0;
      z_order = 0;
      is_activated = 0;
      name = string.Empty;
      events = new string[0];
    }

    public virtual void LoadFromStream(BinaryReader r, int gui_version)
    {
      // parse gui object info
      flags = r.ReadInt32();
      x = r.ReadInt32();
      y = r.ReadInt32();
      width = r.ReadInt32();
      height = r.ReadInt32();
      z_order = r.ReadInt32();

      if (gui_version < 119) // 3.5.0
        is_activated = r.ReadInt32();

      if (gui_version >= 106) // ???
        name = r.ReadCString();

      if (gui_version >= 108) // ???
      {
        Int32 events_count = r.ReadInt32();
        events = new string[events_count];
        for (int i = 0; i < events.Length; ++i)
          events[i] = r.ReadCString();
      }
    }

    public virtual void WriteToStream(BinaryWriter writer, int version)
    {
      writer.Write((Int32)flags);
      writer.Write((Int32)x);
      writer.Write((Int32)y);
      writer.Write((Int32)width);
      writer.Write((Int32)height);
      writer.Write((Int32)z_order);

      if (version < 119) // 3.5.0
        writer.Write((Int32)is_activated);

      if (version >= 106) // ???
        writer.WriteCString(name);

      if (version >= 108) // ???
      {
        writer.Write((Int32)events.Length);
        for (int i = 0; i < events.Length; ++i)
          writer.WriteCString(events[i]);
      }
    }
  }
}
