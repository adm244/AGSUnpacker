
namespace AGSUnpacker.Room
{
  public class AGSRoomEdges
  {
    public int Top;
    public int Bottom;
    public int Left;
    public int Right;

    // TODO(adm244): consider using a constructor with params, so we can use a struct here
    public AGSRoomEdges()
    {
      Top = 0;
      Bottom = 0;
      Left = 0;
      Right = 0;
    }
  }
}
