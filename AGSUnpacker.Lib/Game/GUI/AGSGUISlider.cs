using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Game
{
  public class AGSGUISlider : AGSGUIObject
  {
    public Int32 value_min;
    public Int32 value_max;
    public Int32 value;
    public Int32 is_mouse_pressed;
    public Int32 handle_image;
    public Int32 handle_offset;
    public Int32 background_image;

    public int handle_color;
    public int shadow_color;

    public AGSGUISlider()
    {
      value_min = 0;
      value_max = 0;
      value = 0;
      is_mouse_pressed = 0;
      handle_image = 0;
      handle_offset = 0;
      background_image = 0;

      handle_color = 0;
      shadow_color = 0;
    }

    public override void LoadFromStream(BinaryReader r, int gui_version)
    {
      base.LoadFromStream(r, gui_version);

      // parse slider info
      value_min = r.ReadInt32();
      value_max = r.ReadInt32();
      value = r.ReadInt32();

      if (gui_version < 119) // 3.5.0
      {
        // parse savegame info
        is_mouse_pressed = r.ReadInt32();
      }

      if (gui_version >= 104) // ???
      {
        handle_image = r.ReadInt32();
        handle_offset = r.ReadInt32();
        background_image = r.ReadInt32();
      }
    }

    public override void WriteToStream(BinaryWriter writer, int version)
    {
      base.WriteToStream(writer, version);

      writer.Write((Int32)value_min);
      writer.Write((Int32)value_max);
      writer.Write((Int32)value);

      if (version < 119) // 3.5.0
        writer.Write((Int32)is_mouse_pressed);

      if (version >= 104) // ???
      {
        writer.Write((Int32)handle_image);
        writer.Write((Int32)handle_offset);
        writer.Write((Int32)background_image);
      }
    }

    public override void LoadFromStream_v363(BinaryReader r)
    {
      base.LoadFromStream_v363(r);

      handle_color = r.ReadInt32();
      shadow_color = r.ReadInt32();

      // skip reserved fields
      _ = r.ReadArrayInt32(4);
    }

    public override void WriteToStream_v363(BinaryWriter writer)
    {
      base.WriteToStream_v363(writer);

      writer.Write((Int32)handle_color);
      writer.Write((Int32)shadow_color);

      // nullify reserved fields
      writer.WriteArrayInt32(new int[4]);
    }
  }
}
