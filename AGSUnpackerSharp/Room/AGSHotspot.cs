using System;
using AGSUnpackerSharp.Shared;
using AGSUnpackerSharp.Shared.Interaction;

namespace AGSUnpackerSharp.Room
{
  public class AGSHotspot
  {
    public Int16 walkto_x;
    public Int16 walkto_y;
    public string name;
    public string scriptname;
    public AGSInteractionScript interactions;
    public AGSPropertyStorage properties;

    public AGSInteraction interactions_old;

    public AGSHotspot()
    {
      walkto_x = 0;
      walkto_y = 0;
      name = string.Empty;
      scriptname = string.Empty;
      interactions = new AGSInteractionScript();
      properties = new AGSPropertyStorage();

      interactions_old = new AGSInteraction();
    }
  }
}
