using AGSUnpackerSharp.Shared.Interaction;

namespace AGSUnpackerSharp.Shared
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
