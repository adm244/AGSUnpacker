using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGSUnpackerSharp.Shared;

namespace AGSUnpackerSharp.Room
{
  public class AGSRegion
  {
    public AGSInteractionScript interactions;
    public Int16 light;
    public Int32 tint;

    public AGSRegion()
    {
      interactions = new AGSInteractionScript();
      light = 0;
      tint = 0;
    }
  }
}
