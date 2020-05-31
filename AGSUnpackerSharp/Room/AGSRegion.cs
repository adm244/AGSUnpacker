using AGSUnpackerSharp.Shared;

namespace AGSUnpackerSharp.Room
{
  public class AGSRegion
  {
    public int Light;
    
    //TODO(adm244): AGSRegion: probably convert Tint from Int32 into Color
    public int Tint;

    public AGSInteractions Interactions;

    public AGSRegion()
    {
      Light = 0;
      Tint = 0;
      Interactions = new AGSInteractions();
    }
  }
}
