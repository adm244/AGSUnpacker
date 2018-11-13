using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace AGSUnpackerSharp.Game
{
  public class AGSGUI
  {
    public char[] name;
    public char[] onclick_handler;
    public Int32 x;
    public Int32 y;
    public Int32 width;
    public Int32 height;
    public Int32 control_focus;
    public Int32 controls_count;
    public Int32 popup_style;
    public Int32 popup_at_mouse_y;
    public Int32 background_color;
    public Int32 background_image;
    public Int32 foreground_color;

    public Int32 mouse_over_control;
    public Int32 mouse_was_at_y;
    public Int32 mouse_was_at_x;
    public Int32 mouse_down_control;
    public Int32 highlight_control;

    public Int32 flags;
    public Int32 transparency;
    public Int32 z_order;
    public Int32 id;
    public Int32 padding;
    public Int32 visibility_state;
    public Int32[] control_references;

    public AGSGUI()
    {
      name = new char[0];
      onclick_handler = new char[0];
      x = 0;
      y = 0;
      width = 0;
      height = 0;
      control_focus = 0;
      controls_count = 0;
      popup_style = 0;
      popup_at_mouse_y = 0;
      background_color = 0;
      background_image = 0;
      foreground_color = 0;

      mouse_over_control = 0;
      mouse_was_at_y = 0;
      mouse_was_at_x = 0;
      mouse_down_control = 0;
      highlight_control = 0;

      flags = 0;
      transparency = 0;
      z_order = 0;
      id = 0;
      padding = 0;
      visibility_state = 0;
      control_references = new Int32[0];
    }

    public void LoadFromStream(BinaryReader r)
    {
      //NOTE(adm244): I'm starting to suspect that the source for 3.3.4 Engine.App is
      // actually older than 3.3.4, because it doesn't contain some of these unknown int32's
      Int32 unknown1 = r.ReadInt32();

      name = r.ReadChars(16);
      onclick_handler = r.ReadChars(20);
      x = r.ReadInt32();
      y = r.ReadInt32();
      width = r.ReadInt32();
      height = r.ReadInt32();
      control_focus = r.ReadInt32();
      controls_count = r.ReadInt32();
      popup_style = r.ReadInt32();
      popup_at_mouse_y = r.ReadInt32();
      background_color = r.ReadInt32();
      background_image = r.ReadInt32();
      foreground_color = r.ReadInt32();

      // savegame info
      mouse_over_control = r.ReadInt32();
      mouse_was_at_y = r.ReadInt32();
      mouse_was_at_x = r.ReadInt32();
      mouse_down_control = r.ReadInt32();
      highlight_control = r.ReadInt32();

      flags = r.ReadInt32();
      transparency = r.ReadInt32();
      z_order = r.ReadInt32();
      id = r.ReadInt32();
      padding = r.ReadInt32();

      // skip reserved variables
      r.BaseStream.Seek(5 * sizeof(Int32), SeekOrigin.Current);

      visibility_state = r.ReadInt32();

      // skip "unused" variables
      r.BaseStream.Seek(30 * sizeof(Int32), SeekOrigin.Current);

      control_references = r.ReadArrayInt32(30);
    }
  }
}
