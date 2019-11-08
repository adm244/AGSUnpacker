using System;
using AGSUnpackerSharp.Shared;
using AGSUnpackerSharp.Shared.Interaction;

namespace AGSUnpackerSharp.Room
{
  public class AGSRegion
  {
    public AGSInteractionScript interactions;
    public Int16 light;
    public Int32 tint;

    public AGSInteraction interactions_old;

    public AGSRegion()
    {
      interactions = new AGSInteractionScript();
      light = 0;
      tint = 0;

      interactions_old = new AGSInteraction();
    }
  }
}
