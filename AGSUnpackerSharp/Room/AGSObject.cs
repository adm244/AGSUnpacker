using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGSUnpackerSharp.Shared;

namespace AGSUnpackerSharp.Room
{
  public class AGSObject
  {
    public Int16 sprite;
    public Int16 x;
    public Int16 y;
    public Int16 room;
    public Int16 visible;
    public AGSInteractionScript interactions;
    public Int32 baseline;
    public Int16 flags;
    public string name;
    public string scriptname;
    public AGSProperty properties;

    public AGSObject()
    {
      sprite = 0;
      x = 0;
      y = 0;
      room = 0;
      visible = 0;
      interactions = new AGSInteractionScript();
      baseline = 0;
      flags = 0;
      name = string.Empty;
      scriptname = string.Empty;
      properties = new AGSProperty();
    }
  }
}
