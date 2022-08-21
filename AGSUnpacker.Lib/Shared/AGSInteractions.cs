using System;

using AGSUnpacker.Lib.Shared.Interaction;

namespace AGSUnpacker.Lib.Shared
{
  public class AGSInteractions
  {
    public AGSInteractionScript Script;
    public AGSInteraction Interaction;
    public AGSInteractionLegacy[] InteractionsLegacy;

    public AGSInteractions()
    {
      Script = new AGSInteractionScript();
      Interaction = new AGSInteraction();
      InteractionsLegacy = Array.Empty<AGSInteractionLegacy>();
    }
  }
}
