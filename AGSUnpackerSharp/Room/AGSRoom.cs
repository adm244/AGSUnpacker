using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using AGSUnpackerSharp.Extensions;
using AGSUnpackerSharp.Shared;
using AGSUnpackerSharp.Shared.Interaction;
using AGSUnpackerSharp.Shared.Script;
using AGSUnpackerSharp.Utils;

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
    public string name;
    public Int16 version;
    public Int32 background_bpp;
    public Bitmap[] backgrounds;
    public byte[] paletteShareFlags;
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

    public AGSInteraction interactions_old;
    public string script_text;

    public AGSRoom()
      : this(string.Empty)
    {
    }

    public AGSRoom(string name)
    {
      this.name = name;
      version = 29;
      background_bpp = 1;
      backgrounds = new Bitmap[5];
      paletteShareFlags = new byte[0];
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

      interactions_old = new AGSInteraction();
      script_text = string.Empty;
    }

    public void SaveToFile(string filepath, int room_version)
    {
      FileStream fs = new FileStream(filepath, FileMode.Create);
      BinaryWriter w = new BinaryWriter(fs, Encoding.GetEncoding(1252));

      w.Write((UInt16)room_version);

      WriteRoomBlock(w, 0x01, room_version);

      if (!string.IsNullOrEmpty(script_text))
        WriteRoomBlock(w, 0x02, room_version);

      WriteRoomBlock(w, 0x05, room_version);
      
      if (backgroundFrames > 0)
        WriteRoomBlock(w, 0x06, room_version);

      WriteRoomBlock(w, 0x07, room_version);
      WriteRoomBlock(w, 0x08, room_version);

      if (room_version >= 24)
        WriteRoomBlock(w, 0x09, room_version);

      WriteRoomBlock(w, 0xFF, room_version);

      w.Close();
    }

    public void LoadFromFile(string filepath)
    {
      FileStream fs = new FileStream(filepath, FileMode.Open);
      BinaryReader r = new BinaryReader(fs, Encoding.GetEncoding(1252));

      version = r.ReadInt16();

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
        case 0x02:
          WriteScriptTextBlock(w, room_version);
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
          WritePropertiesBlock(w, properties.version);
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
        case 0x02:
          ParseScriptTextBlock(r, room_version);
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

      if (room_version >= 20) // ???
        w.Write((byte[])paletteShareFlags);

      for (int i = 1; i < backgroundFrames; ++i)
        AGSGraphicUtils.WriteLZ77Image(w, backgrounds[i], background_bpp);
    }

    private void ParseBackgroundAnimationBlock(BinaryReader r, int room_version)
    {
      backgroundFrames = r.ReadByte();
      background_animation_speed = r.ReadByte();

      if (room_version >= 20)
        paletteShareFlags = r.ReadBytes(backgroundFrames);

      for (int i = 1; i < backgroundFrames; ++i)
        backgrounds[i] = AGSGraphicUtils.ReadLZ77Image(r, background_bpp);
    }

    private void WriteObjectScriptNamesBlock(BinaryWriter w, int room_version)
    {
      w.Write((byte)objects.Length);
      for (int i = 0; i < objects.Length; ++i)
      {
        if (room_version >= 31) // 3.4.1.5
          w.WritePrefixedString32(objects[i].scriptname);
        else
          w.WriteFixedString(objects[i].scriptname, 20);
      }
    }

    private void ParseObjectScriptNamesBlock(BinaryReader r, int room_version)
    {
      byte objects_count = r.ReadByte();
      Debug.Assert(objects_count == objects.Length);

      for (int i = 0; i < objects.Length; ++i)
      {
        if (room_version >= 31) // 3.4.1.5
          objects[i].scriptname = r.ReadPrefixedString32();
        else
          objects[i].scriptname = r.ReadFixedString(20);
      }
    }

    private void WriteObjectNamesBlock(BinaryWriter w, int room_version)
    {
      w.Write((byte)objects.Length);

      for (int i = 0; i < objects.Length; ++i)
      {
        if (room_version >= 31) // 3.4.1.5
          w.WritePrefixedString32(objects[i].name);
        else
          w.WriteFixedString(objects[i].name, 30);
      }
    }

    private void ParseObjectNamesBlock(BinaryReader r, int room_version)
    {
      byte objects_count = r.ReadByte();
      Debug.Assert(objects_count == objects.Length);

      for (int i = 0; i < objects.Length; ++i)
      {
        if (room_version >= 31) // 3.4.1.5
          objects[i].name = r.ReadPrefixedString32();
        else
          objects[i].name = r.ReadFixedString(30);
      }
    }

    private void WritePropertiesBlock(BinaryWriter w, int properiesVersion)
    {
      w.Write((Int32)propertiesBlockVersion);
      
      // write room properties
      properties.WriteToStream(w, properiesVersion);

      // write hotspots properties
      for (int i = 0; i < hotspots.Length; ++i)
        hotspots[i].properties.WriteToStream(w, properiesVersion);

      // write objects properies
      for (int i = 0; i < objects.Length; ++i)
        objects[i].properties.WriteToStream(w, properiesVersion);
    }

    private void ParsePropertiesBlock(BinaryReader r)
    {
      propertiesBlockVersion = r.ReadInt32();
      if (propertiesBlockVersion != 1)
        throw new NotImplementedException("CRM: Unknown properties version " + propertiesBlockVersion);

      // parse room properties
      properties.LoadFromStream(r);

      // parse hotspots properties
      for (int i = 0; i < hotspots.Length; ++i)
        hotspots[i].properties.LoadFromStream(r);

      // parse objects properties
      for (int i = 0; i < objects.Length; ++i)
        objects[i].properties.LoadFromStream(r);
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
      if (room_version >= 12) // 2.08+
        w.Write(background_bpp);
      
      // write walk-behind baselines
      w.Write((Int16)walkbehinds.Length);
      for (int i = 0; i < walkbehinds.Length; ++i)
        w.Write((Int16)walkbehinds[i].baseline);

      // write hotspots info
      w.Write((Int32)hotspots.Length);
      for (int i = 0; i < hotspots.Length; ++i)
      {
        w.Write((Int16)hotspots[i].walkto_x);
        w.Write((Int16)hotspots[i].walkto_y);
      }

      for (int i = 0; i < hotspots.Length; ++i)
      {
        if (room_version >= 31) // 3.4.1.5
          w.WritePrefixedString32(hotspots[i].name);
        else if (room_version >= 28) // ???
          w.WriteNullTerminatedString(hotspots[i].name);
        else
          w.WriteFixedString(hotspots[i].name, 30);
      }

      if (room_version >= 24) // ???
      {
        for (int i = 0; i < hotspots.Length; ++i)
        {
          if (room_version >= 31)
            w.WritePrefixedString32(hotspots[i].scriptname);
          else
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

      if (room_version >= 19) // ???
        w.Write((Int32)0x0);

      if (room_version >= 15) // ???
      {
        if (room_version < 26) // ???
        {
          for (int i = 0; i < hotspots.Length; ++i)
            hotspots[i].interactions_old.WriteToStream(w);

          for (int i = 0; i < objects.Length; ++i)
            objects[i].interactions_old.WriteToStream(w);

          interactions_old.WriteToStream(w);
        }

        if (room_version >= 21) // ???
        {
          w.Write((Int32)regions.Length);

          if (room_version < 26) // ???
          {
            for (int i = 0; i < regions.Length; ++i)
              regions[i].interactions_old.WriteToStream(w);
          }
        }

        if (room_version >= 26) // ???
        {
          interactions.WriteToStream(w);

          // write hotspot events
          for (int i = 0; i < hotspots.Length; ++i)
            hotspots[i].interactions.WriteToStream(w);

          // write object events
          for (int i = 0; i < objects.Length; ++i)
            objects[i].interactions.WriteToStream(w);

          // write region events
          for (int i = 0; i < regions.Length; ++i)
            regions[i].interactions.WriteToStream(w);
        }
      }

      if (room_version >= 9) // ???
      {
        // write objects baselines
        for (int i = 0; i < objects.Length; ++i)
          w.Write((Int32)objects[i].baseline);

        // write room dimensions
        w.Write((Int16)width);
        w.Write((Int16)height);
      }

      if (room_version >= 23) // ???
      {
        // write objects flags
        for (int i = 0; i < objects.Length; ++i)
          w.Write((Int16)objects[i].flags);
      }

      if (room_version >= 11) // ???
        w.Write((Int16)resolution_type);

      if (room_version >= 14) // ???
        w.Write((Int32)walkareas.Length);

      if (room_version >= 10) // ???
      {
        for (int i = 0; i < walkareas.Length; ++i)
          w.Write((Int16)walkareas[i].scale_far);
      }

      if (room_version >= 13) // ???
      {
        for (int i = 0; i < walkareas.Length; ++i)
          w.Write((Int16)walkareas[i].light);
      }

      if (room_version >= 18) // ???
      {
        for (int i = 0; i < walkareas.Length; ++i)
          w.Write((Int16)walkareas[i].scale_near);

        for (int i = 0; i < walkareas.Length; ++i)
          w.Write((Int16)walkareas[i].top_y);

        for (int i = 0; i < walkareas.Length; ++i)
          w.Write((Int16)walkareas[i].bottom_y);
      }

      byte[] password_encrypted = new byte[password.Length];
      if (room_version < 9)
      {
        for (int i = 0; i < password_encrypted.Length; ++i)
          password_encrypted[i] = (byte)(password[i] - (byte)60);
      }
      else
      {
        string passwordString = AGSStringUtils.DecryptString(password);
        Encoding windows1252 = Encoding.GetEncoding(1252);
        password_encrypted = windows1252.GetBytes(passwordString);
        Debug.Assert(password_encrypted.Length == password.Length);
      }

      // write room settings
      w.Write(password_encrypted);
      w.Write((byte)startup_music);
      w.Write((byte)saveload_disabled);
      w.Write((byte)player_invisible);
      w.Write((byte)player_view);
      w.Write((byte)music_volume);
      w.Write(new byte[5]);

      w.Write((Int16)messages.Length);

      if (room_version >= 25) // ???
        w.Write((Int32)game_id);

      if (room_version >= 3) // ???
      {
        // write messages flags
        for (int i = 0; i < messages.Length; ++i)
        {
          w.Write((byte)messages[i].display_as);
          w.Write((byte)messages[i].flags);
        }
      }

      // write messages text
      for (int i = 0; i < messages.Length; ++i)
      {
        if (room_version >= 22) // ???
          AGSStringUtils.WriteEncryptedString(w, messages[i].text);
        else
          w.WriteNullTerminatedString(messages[i].text, 2999);
      }

      // write legacy room animations
      if (room_version >= 6)
        w.Write((Int16)0x0);

      if ((room_version >= 4) && (room_version < 16)) // ???
        throw new NotImplementedException("CRM: Legacy graphical scripts writer is not implement.");

      if (room_version >= 8) // ???
      {
        // write walkable areas light level (unused)
        for (int i = 0; i < 16; ++i)
          w.Write((Int16)walkareas[i].light);
      }

      if (room_version >= 21) // ???
      {
        // write regions light level
        for (int i = 0; i < regions.Length; ++i)
          w.Write((Int16)regions[i].light);

        // write regions tint colors
        for (int i = 0; i < regions.Length; ++i)
          w.Write((Int32)regions[i].tint);
      }

      // write primary background
      if (room_version >= 5)
        AGSGraphicUtils.WriteLZ77Image(w, backgrounds[0], background_bpp);
      else
        AGSGraphicUtils.WriteAllegroImage(w, backgrounds[0]);

      // parse region mask
      AGSGraphicUtils.WriteAllegroImage(w, regionMask);

      // parse walkable area mask
      AGSGraphicUtils.WriteAllegroImage(w, walkableMask);

      // parse walkbehind area mask
      AGSGraphicUtils.WriteAllegroImage(w, walkbehindMask);

      // parse hotspot mask
      AGSGraphicUtils.WriteAllegroImage(w, hotspotMask);
    }

    private void ParseRoomMainBlock(BinaryReader r, int room_version)
    {
      if (room_version >= 12) // v2.08+
        background_bpp = r.ReadInt32();
      else
        background_bpp = 1;

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
        //NOTE(adm244): can't really decide which one to use, eh?
        // How about encryption again? Jibzle it! Oh, yeah, it's "open-source" now...
        if (room_version >= 31) // 3.4.1.5
          hotspots[i].name = r.ReadPrefixedString32();
        else if (room_version >= 28) // ???
          hotspots[i].name = r.ReadNullTerminatedString();
        else
          hotspots[i].name = r.ReadFixedString(30);
      }

      if (room_version >= 24) // ???
      {
        for (int i = 0; i < hotspots.Length; ++i)
        {
          if (room_version >= 31) // 3.4.1.5
            hotspots[i].scriptname = r.ReadPrefixedString32();
          else
            hotspots[i].scriptname = r.ReadFixedString(20);
        }
      }

      // parse poly-points
      Int32 polypoints_count = r.ReadInt32();
      if (polypoints_count > 0)
        throw new NotImplementedException("CRM: Polypoints parser is not implemented.");

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

      if (room_version >= 19) // ???
      {
        // parse interaction variables
        Int32 interactionvars_count = r.ReadInt32();
        if (interactionvars_count > 0)
          throw new NotImplementedException("CRM: Interaction variables parser is not implemented.");
      }

      if (room_version >= 15) // ???
      {
        if (room_version < 26) // ???
        {
          for (int i = 0; i < hotspots.Length; ++i)
          {
            hotspots[i].interactions_old = new AGSInteraction();
            hotspots[i].interactions_old.LoadFromStream(r);
          }

          for (int i = 0; i < objects.Length; ++i)
          {
            objects[i].interactions_old = new AGSInteraction();
            objects[i].interactions_old.LoadFromStream(r);
          }

          this.interactions_old = new AGSInteraction();
          this.interactions_old.LoadFromStream(r);
        }

        if (room_version >= 21) // ???
        {
          Int32 regions_count = r.ReadInt32();
          regions = new AGSRegion[regions_count];
          for (int i = 0; i < regions.Length; ++i)
            regions[i] = new AGSRegion();

          if (room_version < 26) // ???
          {
            for (int i = 0; i < regions.Length; ++i)
            {
              regions[i].interactions_old = new AGSInteraction();
              regions[i].interactions_old.LoadFromStream(r);
            }
          }
        }

        if (room_version >= 26) // ???
        {
          // parse room events
          interactions.LoadFromStream(r);

          // parse hotspot events
          for (int i = 0; i < hotspots.Length; ++i)
            hotspots[i].interactions.LoadFromStream(r);

          // parse object events
          for (int i = 0; i < objects.Length; ++i)
            objects[i].interactions.LoadFromStream(r);

          // parse region events
          for (int i = 0; i < regions.Length; ++i)
            regions[i].interactions.LoadFromStream(r);
        }
      }

      if (room_version >= 9) // ???
      {
        // parse objects baselines
        for (int i = 0; i < objects.Length; ++i)
          objects[i].baseline = r.ReadInt32();

        // parse room dimensions
        width = r.ReadInt16();
        height = r.ReadInt16();
      }

      if (room_version >= 23) // ???
      {
        // parse objects flags
        for (int i = 0; i < objects.Length; ++i)
          objects[i].flags = r.ReadInt16();
      }

      if (room_version >= 11) // ???
        resolution_type = r.ReadInt16();

      Int32 walkareas_count = 0;
      if (room_version >= 14) // ???
        walkareas_count = r.ReadInt32();

      walkareas = new AGSWalkableArea[walkareas_count];
      for (int i = 0; i < walkareas.Length; ++i)
        walkareas[i] = new AGSWalkableArea();

      if (room_version >= 10) // ???
      {
        for (int i = 0; i < walkareas.Length; ++i)
          walkareas[i].scale_far = r.ReadInt16();
      }

      if (room_version >= 13) // ???
      {
        for (int i = 0; i < walkareas.Length; ++i)
          walkareas[i].light = r.ReadInt16();
      }

      if (room_version >= 18) // ???
      {
        for (int i = 0; i < walkareas.Length; ++i)
          walkareas[i].scale_near = r.ReadInt16();

        for (int i = 0; i < walkareas.Length; ++i)
          walkareas[i].top_y = r.ReadInt16();

        for (int i = 0; i < walkareas.Length; ++i)
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
      for (int i = 0; i < messages.Length; ++i)
        messages[i] = new AGSMessage();

      if (room_version >= 25) // ???
        game_id = r.ReadInt32();

      if (room_version >= 3) // ???
      {
        // parse messages flags
        for (int i = 0; i < messages.Length; ++i)
        {
          messages[i].display_as = r.ReadByte();
          messages[i].flags = r.ReadByte();
        }
      }

      // parse messages text
      for (int i = 0; i < messages_count; ++i)
      {
        if (room_version >= 22) // ???
          messages[i].text = AGSStringUtils.ReadEncryptedString(r);
        else
          messages[i].text = r.ReadNullTerminatedString(2999);
      }

      if (room_version >= 6) // ???
      {
        // parse legacy room animations
        Int16 room_animations_count = r.ReadInt16();
        if (room_animations_count > 0)
          throw new NotImplementedException("CRM: Legacy room animations parser is not implemented.");
      }

      // parse legacy graphical scripts
      if ((room_version >= 4) && (room_version < 16)) // ???
        throw new NotImplementedException("CRM: Legacy graphical scripts parser is not implemented.");

      if (room_version >= 8) // ???
      {
        // parse walkable areas light level (unused?)
        for (int i = 0; i < walkareas.Length; ++i)
          walkareas[i].light = r.ReadInt16();
      }

      if (room_version >= 21) // ???
      {
        // parse regions light level
        for (int i = 0; i < regions.Length; ++i)
          regions[i].light = r.ReadInt16();

        // parse regions tint colors
        for (int i = 0; i < regions.Length; ++i)
          regions[i].tint = r.ReadInt32();
      }

      // parse primary background
      if (room_version >= 5) // ???
        backgrounds[0] = AGSGraphicUtils.ReadLZ77Image(r, background_bpp);
      else
        backgrounds[0] = AGSGraphicUtils.ReadAllegroImage(r);

      // parse region mask
      regionMask = AGSGraphicUtils.ReadAllegroImage(r);

      // parse walkable area mask
      walkableMask = AGSGraphicUtils.ReadAllegroImage(r);

      // parse walkbehind area mask
      walkbehindMask = AGSGraphicUtils.ReadAllegroImage(r);

      // parse hotspot mask
      hotspotMask = AGSGraphicUtils.ReadAllegroImage(r);

      if (room_version < 9) // ???
      {
        for (int i = 0; i < password.Length; ++i)
          password[i] += 60;
      }
      else
        password = AGSStringUtils.EncryptString(password);
    }

    private void WriteScriptTextBlock(BinaryWriter w, int room_version)
    {
      Encoding windows1252 = Encoding.GetEncoding(1252);
      byte[] buffer = windows1252.GetBytes(script_text);

      buffer = AGSStringUtils.DecryptStringByte(buffer);

      w.Write((Int32)buffer.Length);
      w.Write((byte[])buffer);
    }

    private void ParseScriptTextBlock(BinaryReader r, int room_version)
    {
      Int32 script_length = r.ReadInt32();
      byte[] buffer = r.ReadBytes(script_length);
      script_text = AGSStringUtils.ToString(AGSStringUtils.EncryptString(buffer));
    }
  }
}
