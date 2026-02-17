using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Game
{
  public class AGSGUIButton : AGSGUIObject
  {
    public Int32 image;
    public Int32 image_mouseover;
    public Int32 image_pushed;
    public Int32 image_current;
    public Int32 is_pushed;
    public Int32 is_mouseover;
    public Int32 font;
    public Int32 text_color;
    public Int32 left_click_action;
    public Int32 right_click_action;
    public Int32 left_click_data;
    public Int32 right_click_data;
    public string text;
    public Int32 text_aligment;
    public Int32 reserved1;
    public Int32 padding_x;
    public Int32 padding_y;

    public int button_flags;
    public int shadow_color;
    public int mouseover_back_color;
    public int pushed_back_color;
    public int mouseover_border_color;
    public int pushed_border_color;
    public int mouseover_text_color;
    public int pushed_text_color;

    public AGSGUIButton()
    {
      image = 0;
      image_mouseover = 0;
      image_pushed = 0;
      image_current = 0;
      is_pushed = 0;
      is_mouseover = 0;
      font = 0;
      text_color = 0;
      left_click_action = 0;
      right_click_action = 0;
      left_click_data = 0;
      right_click_data = 0;
      text = string.Empty;
      text_aligment = 0;
      reserved1 = 0;
      padding_x = 0;
      padding_y = 0;

      button_flags = 0;
      shadow_color = 0;
      mouseover_back_color = 0;
      pushed_back_color = 0;
      mouseover_border_color = 0;
      pushed_border_color = 0;
      mouseover_text_color = 0;
      pushed_text_color = 0;
    }

    public override void LoadFromStream(BinaryReader r, int gui_version)
    {
      base.LoadFromStream(r, gui_version);

      // parse button info
      image = r.ReadInt32();
      image_mouseover = r.ReadInt32();
      image_pushed = r.ReadInt32();

      if (gui_version < 119) // 3.5.0
      {
        image_current = r.ReadInt32();
        is_pushed = r.ReadInt32();
        is_mouseover = r.ReadInt32();
      }

      font = r.ReadInt32();
      text_color = r.ReadInt32();
      left_click_action = r.ReadInt32();
      right_click_action = r.ReadInt32();
      left_click_data = r.ReadInt32();
      right_click_data = r.ReadInt32();

      if (gui_version < 119) // 3.5.0
        text = r.ReadFixedCString(50);
      else
        text = r.ReadPrefixedString32();

      if (gui_version >= 111) // 2.7.0+ ???
      {
        text_aligment = r.ReadInt32();
        if (gui_version < 119) // 3.5.0
          reserved1 = r.ReadInt32();
      }
    }

    public override void WriteToStream(BinaryWriter writer, int version)
    {
      base.WriteToStream(writer, version);

      writer.Write((Int32)image);
      writer.Write((Int32)image_mouseover);
      writer.Write((Int32)image_pushed);

      if (version < 119) // 3.5.0
      {
        writer.Write((Int32)image_current);
        writer.Write((Int32)is_pushed);
        writer.Write((Int32)is_mouseover);
      }

      writer.Write((Int32)font);
      writer.Write((Int32)text_color);
      writer.Write((Int32)left_click_action);
      writer.Write((Int32)right_click_action);
      writer.Write((Int32)left_click_data);
      writer.Write((Int32)right_click_data);

      if (version < 119) // 3.5.0
        writer.WriteFixedString(text, 50);
      else
        writer.WritePrefixedString32(text);

      if (version >= 111) // 2.7.0+ ???
      {
        writer.Write((Int32)text_aligment);
        if (version < 119) // 3.5.0
          writer.Write((Int32)reserved1);
      }
    }

    public override void LoadFromStream_v363(BinaryReader r)
    {
      base.LoadFromStream_v363(r);

      button_flags = r.ReadInt32();
      shadow_color = r.ReadInt32();
      mouseover_back_color = r.ReadInt32();
      pushed_back_color = r.ReadInt32();
      mouseover_border_color = r.ReadInt32();
      pushed_border_color = r.ReadInt32();
      mouseover_text_color = r.ReadInt32();
      pushed_text_color = r.ReadInt32();

      // skip reserved fields
      _ = r.ReadArrayInt32(4);
    }

    public override void WriteToStream_v363(BinaryWriter writer)
    {
      base.WriteToStream_v363(writer);

      writer.Write((Int32)button_flags);
      writer.Write((Int32)shadow_color);
      writer.Write((Int32)mouseover_back_color);
      writer.Write((Int32)pushed_back_color);
      writer.Write((Int32)mouseover_border_color);
      writer.Write((Int32)pushed_border_color);
      writer.Write((Int32)mouseover_text_color);
      writer.Write((Int32)pushed_text_color);

      // nullify reserved fields
      writer.WriteArrayInt32(new int[4]);
    }
  }
}
