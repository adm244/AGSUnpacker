using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AGSUnpackerSharp
{
  public class AGSCursorInfo
  {
    public Int32 picture;
    public Int16 hotspot_x;
    public Int16 hotspot_y;
    public Int16 view;
    public string name;
    public Int32 flags;

    public AGSCursorInfo()
    {
      picture = 0;
      hotspot_x = 0;
      hotspot_y = 0;
      view = 0;
      name = string.Empty;
      flags = 0;
    }

    public void LoadFromStream(AGSAlignedStream ar)
    {
      picture = ar.ReadInt32();
      hotspot_x = ar.ReadInt16();
      hotspot_y = ar.ReadInt16();
      view = ar.ReadInt16();
      name = ar.ReadFixedString(10);
      //NOTE(adm244): in engine source it's int8, but in the actual dta file it's an int32
      // might just be a padding issue here, double check that
      flags = ar.ReadInt32();
    }
  }
}
