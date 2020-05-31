using AGSUnpackerSharp.Shared.Interaction;

namespace AGSUnpackerSharp.Shared
{
  public struct AGSInteractions
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
