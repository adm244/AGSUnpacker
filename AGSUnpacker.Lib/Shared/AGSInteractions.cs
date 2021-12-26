using AGSUnpacker.Shared.Interaction;

namespace AGSUnpacker.Shared
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
