using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Game
{
  public class AGSGUITextBox : AGSGUIObject
  {
    public string text;
    public Int32 font;
    public Int32 text_color;
    public Int32 flags;

    public AGSGUITextBox()
    {
      text = string.Empty;
      font = 0;
      text_color = 0;
      flags = 0;
    }

    public override void LoadFromStream(BinaryReader r, int gui_version)
    {
      base.LoadFromStream(r, gui_version);

      // parse textbox info
      if (gui_version < 119) // 3.5.0
        text = r.ReadFixedCString(200);
      else
        text = r.ReadPrefixedString32();

      font = r.ReadInt32();
      text_color = r.ReadInt32();
      flags = r.ReadInt32();
    }

    public override void WriteToStream(BinaryWriter writer, int version)
    {
      base.WriteToStream(writer, version);

      if (version < 119) // 3.5.0
        writer.WriteFixedString(text, 200);
      else
        writer.WritePrefixedString32(text);

      writer.Write((Int32)font);
      writer.Write((Int32)text_color);
      writer.Write((Int32)flags);
    }
  }
}
