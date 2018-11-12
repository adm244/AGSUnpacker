using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGSUnpackerSharp.Shared;

namespace AGSUnpackerSharp.Room
{
  public class AGSHotspot
  {
    public Int16 walkto_x;
    public Int16 walkto_y;
    public string name;
    public char[] scriptname;
    public AGSInteractionScript interactions;
    public AGSProperty properties;

    public AGSHotspot()
    {
      walkto_x = 0;
      walkto_y = 0;
      name = string.Empty;
      scriptname = new char[0];
      interactions = new AGSInteractionScript();
      properties = new AGSProperty();
    }
  }
}
