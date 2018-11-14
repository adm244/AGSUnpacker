using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

    public void LoadFromStream(BinaryReader r)
    {
      base.LoadFromStream(r);

      // parse label info
      Int32 strlen = r.ReadInt32();
      text = r.ReadFixedString(strlen);
      font = r.ReadInt32();
      text_color = r.ReadInt32();
      text_aligment = r.ReadInt32();
    }
  }
}
