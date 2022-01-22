using System.Drawing;

using AGSUnpacker.Lib.Shared;

namespace AGSUnpacker.Lib.Room
{
  public class AGSHotspot
  {
    public string Name;
    public string ScriptName;
    public Point WalkTo;

    public AGSPropertyStorage Properties;
    public AGSInteractions Interactions;

    public AGSHotspot()
    {
      Name = string.Empty;
      ScriptName = string.Empty;
      WalkTo = new Point();

      Properties = new AGSPropertyStorage();
      Interactions = new AGSInteractions();
    }
  }
}
