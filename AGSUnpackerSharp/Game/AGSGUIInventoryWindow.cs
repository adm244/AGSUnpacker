using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AGSUnpackerSharp.Game
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

    public void LoadFromStream(BinaryReader r)
    {
      base.LoadFromStream(r);

      // parse inventory window info
      character_id = r.ReadInt32();
      item_width = r.ReadInt32();
      item_height = r.ReadInt32();

      // parse savegame info
      item_top = r.ReadInt32();
    }
  }
}
