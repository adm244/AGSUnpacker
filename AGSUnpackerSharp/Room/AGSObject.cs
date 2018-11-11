using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGSUnpackerSharp.Shared;

namespace AGSUnpackerSharp.Room
{
  public struct AGSObject
  {
    public Int16 sprite;
    public Int16 x;
    public Int16 y;
    public Int16 room;
    public Int16 visible;
    public AGSInteractionScript events;
    public Int32 baseline;
    public Int16 flags;
    public string name;
    public string scriptname;
  }
}
