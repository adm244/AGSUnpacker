using System.Collections.Generic;

namespace AGSUnpacker.Lib.Room
{
  public class AGSRoomDeprecated
  {
    public AGSEventBlock[] HotspotConditions;
    public AGSEventBlock[] ObjectConditions;
    public AGSEventBlock MiscConditions;

    public AGSRoomDeprecated()
    {
      // NOTE(adm244): 2.00 through 2.40 used these sizes
      HotspotConditions = new AGSEventBlock[20];
      ObjectConditions = new AGSEventBlock[10];
      MiscConditions = null;
    }
  }
}
