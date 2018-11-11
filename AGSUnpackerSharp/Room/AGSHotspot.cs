using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGSUnpackerSharp.Shared;

namespace AGSUnpackerSharp.Room
{
  public struct AGSHotspot
  {
    public Int16 walkto_x;
    public Int16 walkto_y;
    public string name;
    public char[] scriptname;
    public AGSInteractionScript events;
  }
}
