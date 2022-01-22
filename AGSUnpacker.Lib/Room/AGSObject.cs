using System.Drawing;

using AGSUnpacker.Lib.Shared;

namespace AGSUnpacker.Lib.Room
{
  public class AGSObject
  {
    public static readonly int MaxNameLength = 30;
    public static readonly int MaxScriptNameLength = 20;

    public Point Position;
    public bool Visible;
    public int Sprite;
    public int Room;
    public int Baseline;

    //TODO(adm244): AGSObject: define an enum for flags
    public int Flags;

    public string Name;
    public string ScriptName;
    public AGSPropertyStorage Properties;
    public AGSInteractions Interactions;

    public AGSObject()
    {
      Position = Point.Empty;
      Visible = false;
      Sprite = 0;
      Room = 0;
      Baseline = 0;
      Flags = 0;
      Name = string.Empty;
      ScriptName = string.Empty;
      Properties = new AGSPropertyStorage();
      Interactions = new AGSInteractions();
    }
  }
}
