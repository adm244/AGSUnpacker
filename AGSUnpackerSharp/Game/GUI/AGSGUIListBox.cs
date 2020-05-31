using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AGSUnpackerSharp.Game
{
  public class AGSGUIListBox : AGSGUIObject
  {
    public Int32 item_selected;
    public Int32 item_top;
    public Int32 mouse_x;
    public Int32 mouse_y;
    public Int32 row_height;
    public Int32 visible_items_count;
    public Int32 font;
    public Int32 text_color;
    public Int32 text_color_selected;
    public Int32 flags;
    public Int32 text_aligment;
    public Int32 reserved1;
    public Int32 background_color_selected;
    public string[] items;

    public AGSGUIListBox()
    {
      item_selected = 0;
      item_top = 0;
      mouse_x = 0;
      mouse_y = 0;
      row_height = 0;
      visible_items_count = 0;
      font = 0;
      text_color = 0;
      text_color_selected = 0;
      flags = 0;
      text_aligment = 0;
      reserved1 = 0;
      background_color_selected = 0;
      items = new string[0];
    }

    public void LoadFromStream(BinaryReader r, int gui_version)
    {
      base.LoadFromStream(r, gui_version);

      // parse listbox info
      Int32 item_count = r.ReadInt32();
      items = new string[item_count];

      if (gui_version < 119) // 3.5.0
      {
        // parse savegame info
        item_selected = r.ReadInt32();
        item_top = r.ReadInt32();
        mouse_x = r.ReadInt32();
        mouse_y = r.ReadInt32();
        row_height = r.ReadInt32();
        visible_items_count = r.ReadInt32();
      }

      font = r.ReadInt32();
      text_color = r.ReadInt32();
      text_color_selected = r.ReadInt32();
      flags = r.ReadInt32();

      if (gui_version >= 112)
      {
        text_aligment = r.ReadInt32();
        if (gui_version < 119) // 3.5.0
          reserved1 = r.ReadInt32();
      }

      if (gui_version > 107)
        background_color_selected = r.ReadInt32();

      for (int i = 0; i < items.Length; ++i)
      {
        items[i] = r.ReadCString();
      }

      if (gui_version >= 114)
      {
        if (gui_version < 119) // 3.5.0
        {
          if ((flags & 4) != 0)
          {
            // skip savegame info
            r.BaseStream.Seek(item_count * sizeof(Int16), SeekOrigin.Current);
          }
        }
      }
    }
  }
}
