using System;
using System.IO;

namespace AGSUnpackerSharp.Game
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

    public void LoadFromStream(BinaryReader r, int gui_version)
    {
      base.LoadFromStream(r, gui_version);

      // parse label info
      if (gui_version >= 113)
      {
        Int32 strlen = r.ReadInt32();
        text = r.ReadFixedString(strlen);
      }
      else
        text = r.ReadFixedString(200);

      font = r.ReadInt32();
      text_color = r.ReadInt32();
      text_aligment = r.ReadInt32();
    }
  }
}
