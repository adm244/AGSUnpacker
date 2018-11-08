using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGSUnpackerSharp
{
  public struct AGSCursorInfo
  {
    public Int32 picture;
    public Int16 hotspot_x;
    public Int16 hotspot_y;
    public Int16 view;
    public char[] name;
    public byte flags;
  }
}
