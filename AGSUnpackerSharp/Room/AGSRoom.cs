using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using AGSUnpackerSharp.Shared;
using AGSUnpackerSharp.Shared.Script;
using AGSUnpackerSharp.Utils;
using AGSUnpackerSharp.Extensions;
using System.Drawing;

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
    public Int16 version;
    public Int32 background_bpp;
    public Bitmap[] backgrounds;
    public byte backgroundFrames;
    public byte background_animation_speed;
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
    public Bitmap regionMask;
    public Bitmap walkableMask;
    public Bitmap walkbehindMask;
    public Bitmap hotspotMask;
    public Int32 propertiesBlockVersion;

    public AGSRoom()
    {
      version = 29;
      background_bpp = 1;
      backgrounds = new Bitmap[5];
      backgroundFrames = 0;
      background_animation_speed = 4;
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
      regionMask = new Bitmap(width, height);
      walkableMask = new Bitmap(width, height);
      walkbehindMask = new Bitmap(width, height);
      hotspotMask = new Bitmap(width, height);
      propertiesBlockVersion = 1;
    }

    public void SaveToFile(string filepath, int room_version)
    {
      FileStream fs = new FileStream(filepath, FileMode.Create);
      BinaryWriter w = new BinaryWriter(fs, Encoding.GetEncoding(1252));

      w.Write((UInt16)room_version);

      WriteRoomBlock(w, 0x01, room_version);
      WriteRoomBlock(w, 0x05, room_version);
      
      if (backgroundFrames > 0)
        WriteRoomBlock(w, 0x06, room_version);

      WriteRoomBlock(w, 0x07, room_version);
      WriteRoomBlock(w, 0x08, room_version);
      WriteRoomBlock(w, 0x09, room_version);
      WriteRoomBlock(w, 0xFF, room_version);

      w.Close();
    }

    public void LoadFromFile(string filepath)
    {
      FileStream fs = new FileStream(filepath, FileMode.Open);
      BinaryReader r = new BinaryReader(fs, Encoding.GetEncoding(1252));

      version = r.ReadInt16();
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

    private void WriteRoomBlock(BinaryWriter w, byte blockType, int room_version)
    {
      w.Write((byte)blockType);
      if (blockType == 0xFF)
        return;

      w.Write((Int32)0);

      long blockStart = w.BaseStream.Position;

      switch (blockType)
      {
        case 0x01:
          WriteRoomMainBlock(w, room_version);
          break;
        case 0x05:
          WriteObjectNamesBlock(w, room_version);
          break;
        case 0x06:
          WriteBackgroundAnimationBlock(w, room_version);
          break;
        case 0x07:
          WriteSCOM3Block(w, script.Version);
          break;
        case 0x08:
          WritePropertiesBlock(w, propertiesBlockVersion, properties.version);
          break;
        case 0x09:
          WriteObjectScriptNamesBlock(w, room_version);
          break;

        default:
          Debug.Assert(false, "Unknown block is encountered!");
          break;
      }

      long blockEnd = w.BaseStream.Position;
      long blockLength = blockEnd - blockStart;
      Debug.Assert(blockLength < Int32.MaxValue);

      w.BaseStream.Seek(blockStart - sizeof(Int32), SeekOrigin.Begin);
      w.Write((Int32)blockLength);
      w.BaseStream.Seek(blockEnd, SeekOrigin.Begin);
    }

    private void ParseRoomBlock(BinaryReader r, byte blockType, int room_version)
    {
      Int64 length = (room_version < 32) ? r.ReadInt32() : r.ReadInt64();

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
          ParsePropertiesBlock(r);
          break;
        case 0x09:
          ParseObjectScriptNamesBlock(r, room_version);
          break;

        default:
          Debug.Assert(false, "Unknown block is encountered!");
          break;
      }
    }

    private void WriteBackgroundAnimationBlock(BinaryWriter w, int room_version)
    {
      w.Write((byte)backgroundFrames);
      w.Write((byte)background_animation_speed);

      //TODO(adm244): palette share flags
      w.Write(new byte[backgroundFrames]);

      for (int i = 1; i < backgroundFrames; ++i)
      {
        AGSGraphicUtils.WriteLZ77Image(w, backgrounds[i], background_bpp);
      }
    }

    private void ParseBackgroundAnimationBlock(BinaryReader r, int room_version)
    {
      backgroundFrames = r.ReadByte();
      Debug.Assert(backgroundFrames <= 5);

      background_animation_speed = r.ReadByte();
      // skip palette share flags
      r.BaseStream.Seek(backgroundFrames, SeekOrigin.Current);

      for (int i = 1; i < backgroundFrames; ++i)
      {
        backgrounds[i] = AGSGraphicUtils.ParseLZ77Image(r, background_bpp);
      }
    }

    private void WriteObjectScriptNamesBlock(BinaryWriter w, int room_version)
    {
      w.Write((byte)objects.Length);
      for (int i = 0; i < objects.Length; ++i)
      {
        if (room_version >= 31)
        {
          w.WritePrefixedString32(objects[i].scriptname);
        }
        else
        {
          w.WriteFixedString(objects[i].scriptname, 20);
        }
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

    private void WriteObjectNamesBlock(BinaryWriter w, int room_version)
    {
      w.Write((byte)objects.Length);

      for (int i = 0; i < objects.Length; ++i)
      {
        if (room_version >= 31)
        {
          w.Write((Int32)objects[i].name.Length);
          w.Write(objects[i].name.ToCharArray());
        }
        else
        {
          //FIX(adm244): write FIXED string !!!
          //w.WriteNullTerminatedString(objects[i].name, 30);
          w.WriteFixedString(objects[i].name, 30);
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

    private void WritePropertiesBlock(BinaryWriter w, int blockVersion, int properiesVersion)
    {
      w.Write((Int32)blockVersion);
      
      // write room properties
      properties.WriteToStream(w, properiesVersion);

      // write hotspots properties
      for (int i = 0; i < hotspots.Length; ++i)
      {
        hotspots[i].properties.WriteToStream(w, properiesVersion);
      }

      // write objects properies
      for (int i = 0; i < objects.Length; ++i)
      {
        objects[i].properties.WriteToStream(w, properiesVersion);
      }
    }

    private void ParsePropertiesBlock(BinaryReader r)
    {
      propertiesBlockVersion = r.ReadInt32();
      Debug.Assert(propertiesBlockVersion == 1);

      // parse room properties
      properties.LoadFromStream(r);

      // parse hotspots properties
      for (int i = 0; i < hotspots.Length; ++i)
      {
        hotspots[i].properties.LoadFromStream(r);
      }

      // parse objects properties
      for (int i = 0; i < objects.Length; ++i)
      {
        objects[i].properties.LoadFromStream(r);
      }
    }

    private void WriteSCOM3Block(BinaryWriter w, int version)
    {
      script.WriteToStream(w, version);
    }

    private void ParseSCOM3Block(BinaryReader r, int room_version)
    {
      script = new AGSScript();
      script.LoadFromStream(r);
    }

    private void WriteRoomMainBlock(BinaryWriter w, int room_version)
    {
      //w.Write((byte)0x01);
      w.Write(background_bpp);
      
      // write walk-behind baselines
      w.Write((Int16)walkbehinds.Length);
      for (int i = 0; i < walkbehinds.Length; ++i)
      {
        w.Write((Int16)walkbehinds[i].baseline);
      }

      // write hotspots info
      w.Write((Int32)hotspots.Length);
      for (int i = 0; i < hotspots.Length; ++i)
      {
        w.Write((Int16)hotspots[i].walkto_x);
        w.Write((Int16)hotspots[i].walkto_y);
      }

      for (int i = 0; i < hotspots.Length; ++i)
      {
        if (room_version >= 31)
        {
          // DOUBLE CHECK IT!
          w.Write(hotspots[i].name);
        }
        else
        {
          //w.Write(hotspots[i].name.ToCharArray());
          //w.Write((byte)0x0);
          w.WriteNullTerminatedString(hotspots[i].name);
        }
      }

      for (int i = 0; i < hotspots.Length; ++i)
      {
        if (room_version >= 31)
        {
          // DOUBLE CHECK IT!
          w.Write(hotspots[i].scriptname);
        }
        else
        {
          //w.Write(hotspots[i].scriptname.ToCharArray());
          //w.Write((byte)0x0);
          w.WriteFixedString(hotspots[i].scriptname, 20);
        }
      }

      // write polypoints count
      w.Write((Int32)0x0);

      // write room edges
      w.Write((Int16)edge.top);
      w.Write((Int16)edge.bottom);
      w.Write((Int16)edge.left);
      w.Write((Int16)edge.right);

      // write objects info
      w.Write((Int16)objects.Length);
      for (int i = 0; i < objects.Length; ++i)
      {
        w.Write((Int16)objects[i].sprite);
        w.Write((Int16)objects[i].x);
        w.Write((Int16)objects[i].y);
        w.Write((Int16)objects[i].room);
        w.Write((Int16)objects[i].visible);
      }

      // write interaction variables count
      w.Write((Int32)0x0);

      w.Write((Int32)regions.Length);

      interactions.WriteToStream(w);

      // write hotspot events
      for (int i = 0; i < hotspots.Length; ++i)
      {
        hotspots[i].interactions.WriteToStream(w);
      }

      // write object events
      for (int i = 0; i < objects.Length; ++i)
      {
        objects[i].interactions.WriteToStream(w);
      }

      // write region events
      for (int i = 0; i < regions.Length; ++i)
      {
        regions[i].interactions.WriteToStream(w);
      }

      // write objects baselines
      for (int i = 0; i < objects.Length; ++i)
      {
        w.Write((Int32)objects[i].baseline);
      }

      // write room dimensions
      w.Write((Int16)width);
      w.Write((Int16)height);

      // write objects flags
      for (int i = 0; i < objects.Length; ++i)
      {
        w.Write((Int16)objects[i].flags);
      }

      w.Write((Int16)resolution_type);

      // write walkable areas info
      w.Write((Int32)walkareas.Length);
      for (int i = 0; i < walkareas.Length; ++i)
      {
        w.Write((Int16)walkareas[i].scale_far);
      }
      for (int i = 0; i < walkareas.Length; ++i)
      {
        w.Write((Int16)walkareas[i].light);
      }
      for (int i = 0; i < walkareas.Length; ++i)
      {
        w.Write((Int16)walkareas[i].scale_near);
      }
      for (int i = 0; i < walkareas.Length; ++i)
      {
        w.Write((Int16)walkareas[i].top_y);
      }
      for (int i = 0; i < walkareas.Length; ++i)
      {
        w.Write((Int16)walkareas[i].bottom_y);
      }

      // write room settings
      w.Write(password);
      w.Write((byte)startup_music);
      w.Write((byte)saveload_disabled);
      w.Write((byte)player_invisible);
      w.Write((byte)player_view);
      w.Write((byte)music_volume);
      w.Write(new byte[5]);

      w.Write((Int16)messages.Length);

      w.Write((Int32)game_id);

      // write messages flags
      for (int i = 0; i < messages.Length; ++i)
      {
        w.Write((byte)messages[i].display_as);
        w.Write((byte)messages[i].flags);
      }

      // write messages text
      for (int i = 0; i < messages.Length; ++i)
      {
        AGSStringUtils.WriteEncryptedString(w, messages[i].text);
      }

      // write legacy room animations
      w.Write((Int16)0x0);

      // write walkable areas light level (unused)
      for (int i = 0; i < 16; ++i)
      {
        w.Write((Int16)walkareas[i].light);
      }
      // write regions light level
      for (int i = 0; i < regions.Length; ++i)
      {
        w.Write((Int16)regions[i].light);
      }
      // write regions tint colors
      for (int i = 0; i < regions.Length; ++i)
      {
        w.Write((Int32)regions[i].tint);
      }

      // write primary background
      AGSGraphicUtils.WriteLZ77Image(w, backgrounds[0], background_bpp);

      // parse region mask
      AGSGraphicUtils.WriteAllegroCompressedImage(w, regionMask);

      // parse walkable area mask
      AGSGraphicUtils.WriteAllegroCompressedImage(w, walkableMask);

      // parse walkbehind area mask
      AGSGraphicUtils.WriteAllegroCompressedImage(w, walkbehindMask);

      // parse hotspot mask
      AGSGraphicUtils.WriteAllegroCompressedImage(w, hotspotMask);
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
        messages[i].text = AGSStringUtils.ReadEncryptedString(r);
      }

      // parse legacy room animations
      Int16 room_animations_count = r.ReadInt16();
      Debug.Assert(room_animations_count == 0);

      // parse walkable areas light level (unused?)
      for (int i = 0; i < walkareas.Length; ++i)
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

      // parse primary background
      backgrounds[0] = AGSGraphicUtils.ParseLZ77Image(r, background_bpp);
      //Debug.Assert(r.BaseStream.Position == 0x54A1);

      // parse region mask
      regionMask = AGSGraphicUtils.ParseAllegroCompressedImage(r);
      //Debug.Assert(r.BaseStream.Position == 0x5C55);

      // parse walkable area mask
      walkableMask = AGSGraphicUtils.ParseAllegroCompressedImage(r);

      // parse walkbehind area mask
      walkbehindMask = AGSGraphicUtils.ParseAllegroCompressedImage(r);

      // parse hotspot mask
      hotspotMask = AGSGraphicUtils.ParseAllegroCompressedImage(r);
      //Debug.Assert(r.BaseStream.Position == 0x7655);
    }
  }
}
