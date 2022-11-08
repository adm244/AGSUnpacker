using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Game
{
  public class AGSGUI
  {
    public string name;
    public string onclick_handler;
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

    public Int32 TextWindowTag;

    public AGSGUI()
    {
      name = string.Empty;
      onclick_handler = string.Empty;
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

      TextWindowTag = 0;
    }

    public void LoadFromStream(BinaryReader r, int gui_version)
    {
      if (gui_version < 119) // 3.5.0
        TextWindowTag = r.ReadInt32();

      if (gui_version < 118) // 3.4.0
      {
        name = r.ReadFixedCString(16);
        onclick_handler = r.ReadFixedCString(20);
      }
      else
      {
        // FIXME(adm244): double check this
        //FIX(adm244): why ReadString is not working?
        // Int32 strlen = r.ReadInt32();
        // name = r.ReadFixedCString(strlen);
        name = r.ReadPrefixedString32();

        //strlen = r.ReadInt32();
        //onclick_handler = r.ReadFixedCString(strlen);
        onclick_handler = r.ReadPrefixedString32();
      }

      x = r.ReadInt32();
      y = r.ReadInt32();
      width = r.ReadInt32();
      height = r.ReadInt32();

      if (gui_version < 119) // 3.5.0
        control_focus = r.ReadInt32();

      controls_count = r.ReadInt32();
      popup_style = r.ReadInt32();
      popup_at_mouse_y = r.ReadInt32();
      background_color = r.ReadInt32();
      background_image = r.ReadInt32();
      foreground_color = r.ReadInt32();

      if (gui_version < 119) // 3.5.0
      {
        // savegame info
        mouse_over_control = r.ReadInt32();
        mouse_was_at_y = r.ReadInt32();
        mouse_was_at_x = r.ReadInt32();
        mouse_down_control = r.ReadInt32();
        highlight_control = r.ReadInt32();
      }

      flags = r.ReadInt32();
      transparency = r.ReadInt32();
      z_order = r.ReadInt32();
      id = r.ReadInt32();
      // FIXME(adm244): is this a structure padding or padding used by GUI?
      padding = r.ReadInt32();

      if (gui_version < 119) // 3.5.0
      {
        // skip reserved variables
        r.BaseStream.Seek(5 * sizeof(Int32), SeekOrigin.Current);
        visibility_state = r.ReadInt32();
      }

      if (gui_version < 118) // 3.4.0
      {
        // skip "unused" variables
        r.BaseStream.Seek(30 * sizeof(Int32), SeekOrigin.Current);
        control_references = r.ReadArrayInt32(30);
      }
      else
      {
        if (controls_count > 0)
          control_references = r.ReadArrayInt32(controls_count);
      }
    }

    public void WriteToStream(BinaryWriter writer, int version)
    {
      if (version < 119) // 3.5.0
        writer.Write((Int32)TextWindowTag);

      if (version < 118) // 3.4.0
      {
        writer.WriteFixedString(name, 16);
        writer.WriteFixedString(onclick_handler, 20);
      }
      else
      {
        writer.WritePrefixedString32(name);
        writer.WritePrefixedString32(onclick_handler);
      }

      writer.Write((Int32)x);
      writer.Write((Int32)y);
      writer.Write((Int32)width);
      writer.Write((Int32)height);

      if (version < 119) // 3.5.0
        writer.Write((Int32)control_focus);

      writer.Write((Int32)controls_count);
      writer.Write((Int32)popup_style);
      writer.Write((Int32)popup_at_mouse_y);
      writer.Write((Int32)background_color);
      writer.Write((Int32)background_image);
      writer.Write((Int32)foreground_color);

      if (version < 119) // 3.5.0
      {
        writer.Write((Int32)mouse_over_control);
        writer.Write((Int32)mouse_was_at_y);
        writer.Write((Int32)mouse_was_at_x);
        writer.Write((Int32)mouse_down_control);
        writer.Write((Int32)highlight_control);
      }

      writer.Write((Int32)flags);
      writer.Write((Int32)transparency);
      writer.Write((Int32)z_order);
      writer.Write((Int32)id);
      // FIXME(adm244): is this a structure padding or padding used by GUI?
      writer.Write((Int32)padding);

      if (version < 119) // 3.5.0
      {
        writer.WriteArrayInt32(new Int32[5]); // reserved
        writer.Write((Int32)visibility_state);
      }

      if (version < 118) // 3.4.0
      {
        writer.WriteArrayInt32(new Int32[30]); // reserved
        writer.WriteArrayInt32(control_references);
      }
      else
      {
        if (controls_count > 0)
          writer.WriteArrayInt32(control_references);
      }
    }
  }
}
