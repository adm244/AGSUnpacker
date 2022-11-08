using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Game
{
  public class AGSGUILabel : AGSGUIObject
  {
    public string text;
    public Int32 font;
    public Int32 text_color;
    public Int32 text_aligment;

    public AGSGUILabel()
    {
      text = string.Empty;
      font = 0;
      text_color = 0;
      text_aligment = 0;
    }

    public override void LoadFromStream(BinaryReader r, int gui_version)
    {
      base.LoadFromStream(r, gui_version);

      // parse label info
      if (gui_version >= 113)
      {
        // NOTE(adm244): label strings are prefixed AND null-terminated...
        // maybe we should check all strings AGS spits for null-terminator from now on?
        int strlen = r.ReadInt32();
        text = r.ReadFixedCString(strlen);
      }
      else
        text = r.ReadFixedCString(200);

      font = r.ReadInt32();
      text_color = r.ReadInt32();
      text_aligment = r.ReadInt32();
    }

    public override void WriteToStream(BinaryWriter writer, int version)
    {
      base.WriteToStream(writer, version);

      if (version >= 113) // ???
      {
        // FIXME(adm244): +1 because our codebase is just as good...
        writer.Write((Int32)text.Length + 1);
        writer.WriteFixedCString(text, text.Length);
      }
      else
        writer.WriteFixedString(text, 200);

      writer.Write((Int32)font);
      writer.Write((Int32)text_color);
      writer.Write((Int32)text_aligment);
    }
  }
}
