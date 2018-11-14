using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AGSUnpackerSharp.Shared;
using System.Diagnostics;

namespace AGSUnpackerSharp.Room
{
  public struct AGSRoomEdge
  {
    public Int16 top;
    public Int16 bottom;
    public Int16 left;
    public Int16 right;
  }

  public class AGSRoom
  {
    public Int32 background_bpp;
    public AGSWalkBehindArea[] walkbehinds;
    public AGSHotspot[] hotspots;
    public AGSRoomEdge edge;
    public AGSObject[] objects;
    public AGSInteractionScript interactions;
    public AGSRegion[] regions;
    public Int16 width;
    public Int16 height;
    public Int16 resolution_type;
    public AGSWalkableArea[] walkareas;
    public byte[] password;
    public byte startup_music;
    public byte saveload_disabled;
    public byte player_invisible;
    public byte player_view;
    public byte music_volume;
    public AGSMessage[] messages;
    public Int32 game_id;
    public AGSScript script;
    public AGSPropertyStorage properties;

    public AGSRoom()
    {
      background_bpp = 1;
      walkbehinds = new AGSWalkBehindArea[0];
      hotspots = new AGSHotspot[0];
      edge.top = 0;
      edge.bottom = 0;
      edge.left = 0;
      edge.right = 0;
      objects = new AGSObject[0];
      interactions = new AGSInteractionScript();
      regions = new AGSRegion[0];
      width = 320;
      height = 200;
      resolution_type = 1;
      walkareas = new AGSWalkableArea[0];
      password = new byte[0];
      startup_music = 0;
      saveload_disabled = 0;
      player_invisible = 0;
      player_view = 0;
      music_volume = 0;
      messages = new AGSMessage[0];
      game_id = 0;
      script = new AGSScript();
      properties = new AGSPropertyStorage();
    }

    public void LoadFromFile(string filepath)
    {
      FileStream fs = new FileStream(filepath, FileMode.Open);
      BinaryReader r = new BinaryReader(fs, Encoding.ASCII);

      Int16 version = r.ReadInt16();
      //Debug.Assert(version == 29);

      byte blockType = 0xFF;
      do
      {
        blockType = r.ReadByte();
        if (blockType != 0xFF)
        {
          ParseRoomBlock(r, blockType, version);
        }
      } while (blockType != 0xFF);

      r.Close();
    }

    private void ParseRoomBlock(BinaryReader r, byte blockType, int room_version)
    {
      Int32 length = r.ReadInt32();

      switch (blockType)
      {
        case 0x01:
          ParseRoomMainBlock(r, room_version);
          break;
        case 0x05:
          ParseObjectNamesBlock(r, room_version);
          break;
        case 0x06:
          ParseBackgroundAnimationBlock(r, room_version);
          break;
        case 0x07:
          ParseSCOM3Block(r, room_version);
          break;
        case 0x08:
          ParsePropertiesBlock(r, room_version);
          break;
        case 0x09:
          ParseObjectScriptNamesBlock(r, room_version);
          break;

        default:
          Debug.Assert(false, "Unknown block is encountered!");
          break;
      }
    }

    private void ParseBackgroundAnimationBlock(BinaryReader r, int room_version)
    {
      byte frames = r.ReadByte();
      Debug.Assert(frames <= 5);

      byte animation_speed = r.ReadByte();
      // skip palette share flags
      r.BaseStream.Seek(frames, SeekOrigin.Current);

      for (int i = 1; i < frames; ++i)
      {
        ParseLZWImage(r);
      }
    }

    private void ParseObjectScriptNamesBlock(BinaryReader r, int room_version)
    {
      byte objects_count = r.ReadByte();
      Debug.Assert(objects_count == objects.Length);

      for (int i = 0; i < objects.Length; ++i)
      {
        if (room_version >= 31) // 3.4.1.5
        {
          Int32 strlen = r.ReadInt32();
          objects[i].scriptname = r.ReadFixedString(strlen);
        }
        else
        {
          objects[i].scriptname = r.ReadFixedString(20);
        }
      }
    }

    private void ParseObjectNamesBlock(BinaryReader r, int room_version)
    {
      byte objects_count = r.ReadByte();
      Debug.Assert(objects_count == objects.Length);

      for (int i = 0; i < objects.Length; ++i)
      {
        if (room_version >= 31) // 3.4.1.5
        {
          Int32 strlen = r.ReadInt32();
          objects[i].name = r.ReadFixedString(strlen);
        }
        else
        {
          objects[i].name = r.ReadFixedString(30);
        }
      }
    }

    private void ParsePropertiesBlock(BinaryReader r, int room_version)
    {
      Int32 version = r.ReadInt32();
      Debug.Assert(version == 1);

      // parse room properties
      properties.LoadFromStream(r);

      // parse hotspot properties
      for (int i = 0; i < hotspots.Length; ++i)
      {
        hotspots[i].properties.LoadFromStream(r);
      }

      // parse object properties
      for (int i = 0; i < objects.Length; ++i)
      {
        objects[i].properties.LoadFromStream(r);
      }
    }

    private void ParseSCOM3Block(BinaryReader r, int room_version)
    {
      script = new AGSScript();
      script.LoadFromStream(r);
    }

    private void ParseRoomMainBlock(BinaryReader r, int room_version)
    {
      background_bpp = r.ReadInt32();

      // parse walk-behind baselines
      Int16 walkbehind_count = r.ReadInt16();
      walkbehinds = new AGSWalkBehindArea[walkbehind_count];
      for (int i = 0; i < walkbehinds.Length; ++i)
      {
        walkbehinds[i] = new AGSWalkBehindArea();
        walkbehinds[i].baseline = r.ReadInt16();
      }

      // parse hotspots info
      Int32 hotspots_count = r.ReadInt32();
      hotspots = new AGSHotspot[hotspots_count];
      for (int i = 0; i < hotspots.Length; ++i)
      {
        hotspots[i] = new AGSHotspot();
        hotspots[i].walkto_x = r.ReadInt16();
        hotspots[i].walkto_y = r.ReadInt16();
      }
      for (int i = 0; i < hotspots.Length; ++i)
      {
        if (room_version >= 31) // 3.4.1.5
        {
          //NOTE(adm244): why they switched from null-terminated string to this one??
          Int32 strlen = r.ReadInt32();
          hotspots[i].name = r.ReadFixedString(strlen);
        }
        else
        {
          hotspots[i].name = r.ReadNullTerminatedString();
        }
      }
      for (int i = 0; i < hotspots.Length; ++i)
      {
        if (room_version >= 31) // 3.4.1.5
        {
          //NOTE(adm244): why they switched from null-terminated string to this one??
          Int32 strlen = r.ReadInt32();
          hotspots[i].scriptname = r.ReadFixedString(strlen);
        }
        else
        {
          hotspots[i].scriptname = r.ReadFixedString(20);
        }
      }

      // parse poly-points
      Int32 polypoints_count = r.ReadInt32();
      Debug.Assert(polypoints_count == 0);

      // parse room edges
      edge.top = r.ReadInt16();
      edge.bottom = r.ReadInt16();
      edge.left = r.ReadInt16();
      edge.right = r.ReadInt16();

      // parse room objects
      Int16 objects_count = r.ReadInt16();
      objects = new AGSObject[objects_count];
      for (int i = 0; i < objects_count; ++i)
      {
        objects[i] = new AGSObject();
        objects[i].sprite = r.ReadInt16();
        objects[i].x = r.ReadInt16();
        objects[i].y = r.ReadInt16();
        objects[i].room = r.ReadInt16();
        objects[i].visible = r.ReadInt16();
      }

      // parse interaction variables
      Int32 interactionvars_count = r.ReadInt32();
      Debug.Assert(interactionvars_count == 0);

      Int32 regions_count = r.ReadInt32();
      regions = new AGSRegion[regions_count];

      // parse room events
      interactions.LoadFromStream(r);

      // parse hotspot events
      for (int i = 0; i < hotspots.Length; ++i)
      {
        hotspots[i].interactions.LoadFromStream(r);
      }

      // parse object events
      for (int i = 0; i < objects.Length; ++i)
      {
        objects[i].interactions.LoadFromStream(r);
      }

      // parse region events
      for (int i = 0; i < regions.Length; ++i)
      {
        regions[i] = new AGSRegion();
        regions[i].interactions.LoadFromStream(r);
      }

      // parse objects baselines
      for (int i = 0; i < objects.Length; ++i)
      {
        objects[i].baseline = r.ReadInt32();
      }

      // parse room dimensions
      width = r.ReadInt16();
      height = r.ReadInt16();

      // parse objects flags
      for (int i = 0; i < objects.Length; ++i)
      {
        objects[i].flags = r.ReadInt16();
      }

      resolution_type = r.ReadInt16();

      // parse walkable areas info
      Int32 walkareas_count = r.ReadInt32();
      walkareas = new AGSWalkableArea[walkareas_count];
      for (int i = 0; i < walkareas.Length; ++i)
      {
        walkareas[i] = new AGSWalkableArea();
        walkareas[i].scale_far = r.ReadInt16();
      }
      for (int i = 0; i < walkareas.Length; ++i)
      {
        walkareas[i].light = r.ReadInt16();
      }
      for (int i = 0; i < walkareas.Length; ++i)
      {
        walkareas[i].scale_near = r.ReadInt16();
      }
      for (int i = 0; i < walkareas.Length; ++i)
      {
        walkareas[i].top_y = r.ReadInt16();
      }
      for (int i = 0; i < walkareas.Length; ++i)
      {
        walkareas[i].bottom_y = r.ReadInt16();
      }

      // parse room settings
      password = r.ReadBytes(11);
      startup_music = r.ReadByte();
      saveload_disabled = r.ReadByte();
      player_invisible = r.ReadByte();
      player_view = r.ReadByte();
      music_volume = r.ReadByte();
      r.BaseStream.Seek(5, SeekOrigin.Current);

      Int16 messages_count = r.ReadInt16();
      messages = new AGSMessage[messages_count];

      game_id = r.ReadInt32();

      // parse messages flags
      for (int i = 0; i < messages.Length; ++i)
      {
        messages[i].display_as = r.ReadByte();
        messages[i].flags = r.ReadByte();
      }

      // parse messages text
      for (int i = 0; i < messages_count; ++i)
      {
        messages[i].text = AGSUtils.ReadEncryptedString(r);
      }

      // parse legacy room animations
      Int16 room_animations_count = r.ReadInt16();
      Debug.Assert(room_animations_count == 0);

      // parse walkable areas light level (unused)
      for (int i = 0; i < 16; ++i)
      {
        walkareas[i].light = r.ReadInt16();
      }
      // parse regions light level
      for (int i = 0; i < regions.Length; ++i)
      {
        regions[i].light = r.ReadInt16();
      }
      // parse regions tint colors
      for (int i = 0; i < regions.Length; ++i)
      {
        regions[i].tint = r.ReadInt32();
      }
      //Debug.Assert(r.BaseStream.Position == 0xC22);

      //TODO(adm244): parse stuff below as well

      // parse primary background
      ParseLZWImage(r);
      //Debug.Assert(r.BaseStream.Position == 0x54A1);

      // parse region mask
      ParseAllegroCompressedImage(r);
      //Debug.Assert(r.BaseStream.Position == 0x5C55);

      // parse walkable area mask
      ParseAllegroCompressedImage(r);

      // parse walkbehind area mask
      ParseAllegroCompressedImage(r);

      // parse hotspot mask
      ParseAllegroCompressedImage(r);
      //Debug.Assert(r.BaseStream.Position == 0x7655);
    }

    //TODO(adm244): move to some other place
    private void ParseAllegroCompressedImage(BinaryReader r)
    {
      Int16 width = r.ReadInt16();
      Int16 height = r.ReadInt16();

      //TODO(adm244): do real parsing
      for (int y = 0; y < height; ++y)
      {
        int pixelsRead = 0;
        while (pixelsRead < width)
        {
          sbyte index = (sbyte)r.ReadByte();
          if (index == -128) index = 0;

          if (index < 0)
          {
            r.BaseStream.Seek(1, SeekOrigin.Current);
            pixelsRead += (1 - index);
          }
          else
          {
            r.BaseStream.Seek(index + 1, SeekOrigin.Current);
            pixelsRead += (index + 1);
          }
        }
      }

      // skip palette
      // 768 = 256 * 3
      r.BaseStream.Seek(768, SeekOrigin.Current);
    }

    //TODO(adm244): move to some other place
    private void ParseLZWImage(BinaryReader r)
    {
      // skip palette
      r.BaseStream.Seek(256 * sizeof(Int32), SeekOrigin.Current);
      Int32 picture_maxsize = r.ReadInt32();
      Int32 picture_data_size = r.ReadInt32();
      // skip lzw compressed image
      r.BaseStream.Seek(picture_data_size, SeekOrigin.Current);
    }
  }
}
