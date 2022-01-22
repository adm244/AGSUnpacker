using AGSUnpacker.Lib.Shared.Interaction;

namespace AGSUnpacker.Lib.Shared
{
  public class AGSInteractions
  {
    public AGSInteractionScript Script;
    public AGSInteraction Interaction;

    public AGSInteractions()
    {
      Script = new AGSInteractionScript();
      Interaction = new AGSInteraction();
    }
  }
}
