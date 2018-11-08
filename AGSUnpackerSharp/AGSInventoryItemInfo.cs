using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGSUnpackerSharp
{
  public struct AGSInventoryItemInfo
  {
    public char[] name;
    public Int32 picture;
    public Int32 cursor_picture;
    public Int32 hotspot_x;
    public Int32 hotspot_y;
    public Int32[] reserved;
    public Int32 flag;
  }
}
