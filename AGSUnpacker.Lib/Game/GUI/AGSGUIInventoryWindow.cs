using System;
using System.IO;

namespace AGSUnpacker.Lib.Game
{
  public class AGSGUIInventoryWindow : AGSGUIObject
  {
    public Int32 character_id;
    public Int32 item_width;
    public Int32 item_height;
    public Int32 item_top;

    public AGSGUIInventoryWindow()
    {
      character_id = 0;
      item_width = 0;
      item_height = 0;
      item_top = 0;
    }

    public override void LoadFromStream(BinaryReader r, int gui_version)
    {
      base.LoadFromStream(r, gui_version);

      if (gui_version > 109) // ???
      {
        // parse inventory window info
        character_id = r.ReadInt32();
        item_width = r.ReadInt32();
        item_height = r.ReadInt32();

        if (gui_version < 119) // 3.5.0
        {
          // parse savegame info
          item_top = r.ReadInt32();
        }
      }
      else
      {
        character_id = -1;
        item_width = 40;
        item_height = 22;
        item_top = 0;
      }
    }

    public override void WriteToStream(BinaryWriter writer, int version)
    {
      base.WriteToStream(writer, version);

      if (version > 109) // ???
      {
        writer.Write((Int32)character_id);
        writer.Write((Int32)item_width);
        writer.Write((Int32)item_height);

        if (version < 119) // 3.5.0
          writer.Write((Int32)item_top);
      }
    }
  }
}
