using System.Threading.Tasks;

using AGSUnpacker.Lib.Room;
using AGSUnpacker.UI.Models.Room;

namespace AGSUnpacker.UI.Services
{
  internal static class ModelService
  {
    public async static Task<Room> LoadRoomAsync(string filepath)
    {
      AGSRoom agsRoom = new AGSRoom();

      await Task.Run(
        () => agsRoom.ReadFromFileDeprecated(filepath)
      );

      return new Room(agsRoom);
    }

    public async static Task SaveRoomAsync(string filepath, Room room)
    {
      await Task.Run(
        () => room.BaseRoom.WriteToFile(filepath, room.BaseRoom.Version)
      );
    }
  }
}
