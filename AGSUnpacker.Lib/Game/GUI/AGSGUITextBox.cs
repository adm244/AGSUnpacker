using System;
using System.IO;

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

    public void LoadFromStream(BinaryReader r, int gui_version)
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
  }
}
