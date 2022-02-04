
using AGSUnpacker.Lib.Room;

namespace AGSUnpacker.UI.Models.Room
{
  internal class Room
  {
    public AGSRoom BaseRoom { get; }

    public RoomBackground Background { get; }

    public Room(AGSRoom agsRoom)
    {
      BaseRoom = agsRoom;
      Background = new RoomBackground(agsRoom.Background);
    }

    public void ChangeFrame(int index, Graphics.Bitmap bitmap)
    {
      BaseRoom.Background.Frames[index] = bitmap;
      Background.ChangeFrame(index, bitmap);
    }
  }
}
