using System;
using System.IO;

using AGSUnpacker.Lib.Shared;

namespace AGSUnpacker.Lib.Room
{
  public class AGSRoomProperties
  {
    private AGSRoomMarkup RoomMarkup;

    public int Version;
    public AGSPropertyStorage Storage;

    public AGSRoomProperties(AGSRoomMarkup roomMarkup)
    {
      RoomMarkup = roomMarkup;

      Version = 1;
      Storage = new AGSPropertyStorage();
    }

    public void ReadBlock(BinaryReader reader, int roomVersion)
    {
      Version = reader.ReadInt32();
      if (Version != 1)
        throw new NotImplementedException("CRM: Unknown properties version " + Version);

      // parse room properties
      Storage.ReadFromStream(reader);

      // parse hotspots properties
      for (int i = 0; i < RoomMarkup.Hotspots.Length; ++i)
        RoomMarkup.Hotspots[i].Properties.ReadFromStream(reader);

      // parse objects properties
      for (int i = 0; i < RoomMarkup.Objects.Length; ++i)
        RoomMarkup.Objects[i].Properties.ReadFromStream(reader);
    }

    public void WriteBlock(BinaryWriter writer, int roomVersion)
    {
      writer.Write((Int32)Version);

      // write room properties
      Storage.WriteToStream(writer);

      // write hotspots properties
      for (int i = 0; i < RoomMarkup.Hotspots.Length; ++i)
        RoomMarkup.Hotspots[i].Properties.WriteToStream(writer);

      // write objects properies
      for (int i = 0; i < RoomMarkup.Objects.Length; ++i)
        RoomMarkup.Objects[i].Properties.WriteToStream(writer);
    }
  }
}
