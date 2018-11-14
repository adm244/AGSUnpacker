using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AGSUnpackerSharp.Game
{
  public class AGSGUITextBox : AGSGUIObject
  {
    public char[] text;
    public Int32 font;
    public Int32 text_color;
    public Int32 flags;

    public AGSGUITextBox()
    {
      text = new char[0];
      font = 0;
      text_color = 0;
      flags = 0;
    }

    public void LoadFromStream(BinaryReader r)
    {
      base.LoadFromStream(r);

      // parse textbox info
      text = r.ReadChars(200);
      font = r.ReadInt32();
      text_color = r.ReadInt32();
      flags = r.ReadInt32();
    }
  }
}
