using System.Diagnostics;
using System.IO;
using AGSUnpackerSharp.Extensions;

namespace AGSUnpackerSharp.Room
{
  public class AGSRoomMarkup
  {
    public AGSWalkableArea[] WalkableAreas;
    public AGSWalkbehindArea[] WalkbehindAreas;
    public AGSHotspot[] Hotspots;
    public AGSRegion[] Regions;
    public AGSObject[] Objects;

    public AGSRoomMarkup()
    {
      WalkableAreas = new AGSWalkableArea[0];
      WalkbehindAreas = new AGSWalkbehindArea[0];
      Hotspots = new AGSHotspot[0];
      Regions = new AGSRegion[0];
      Objects = new AGSObject[0];
    }

    public void ReadObjectScriptNamesBlock(BinaryReader r, int room_version)
    {
      byte count = r.ReadByte();
      Debug.Assert(count == Objects.Length);

      for (int i = 0; i < Objects.Length; ++i)
      {
        if (room_version >= 31) // 3.4.1.5
          Objects[i].ScriptName = r.ReadPrefixedString32();
        else
          Objects[i].ScriptName = r.ReadFixedCString(AGSObject.MaxScriptNameLength);
      }
    }

    public void WriteObjectScriptNamesBlock(BinaryWriter writer, int roomVersion)
    {
      writer.Write((byte)Objects.Length);

      for (int i = 0; i < Objects.Length; ++i)
      {
        if (roomVersion >= 31) // 3.4.1.5
          writer.WritePrefixedString32(Objects[i].ScriptName);
        else
          writer.WriteFixedString(Objects[i].ScriptName, AGSObject.MaxScriptNameLength);
      }
    }

    public void ReadObjectNamesBlock(BinaryReader reader, int roomVersion)
    {
      byte count = reader.ReadByte();
      Debug.Assert(count == Objects.Length);

      for (int i = 0; i < Objects.Length; ++i)
      {
        if (roomVersion >= 31) // 3.4.1.5
          Objects[i].Name = reader.ReadPrefixedString32();
        else
          Objects[i].Name = reader.ReadFixedCString(AGSObject.MaxNameLength);
      }
    }

    public void WriteObjectNamesBlock(BinaryWriter writer, int roomVersion)
    {
      writer.Write((byte)Objects.Length);

      for (int i = 0; i < Objects.Length; ++i)
      {
        if (roomVersion >= 31) // 3.4.1.5
          writer.WritePrefixedString32(Objects[i].Name);
        else
          writer.WriteFixedString(Objects[i].Name, AGSObject.MaxNameLength);
      }
    }
  }
}
