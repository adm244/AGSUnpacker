using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using AGSUnpacker.Lib.Graphics;
using AGSUnpacker.Lib.Shared;
using AGSUnpacker.Lib.Shared.FormatExtensions;
using AGSUnpacker.Lib.Shared.Interaction;
using AGSUnpacker.Shared.Extensions;
using AGSUnpacker.Shared.Utils.Encryption;

namespace AGSUnpacker.Lib.Room
{
  public class AGSRoom
  {
    /*
    (1.00,  1) BLOCKTYPE_MAIN        1
    ( ????, 6) BLOCKTYPE_SCRIPT      2
    ( ?????? ) BLOCKTYPE_COMPSCRIPT  3
    ( ?????? ) BLOCKTYPE_COMPSCRIPT2 4
    ( ?????? ) BLOCKTYPE_OBJECTNAMES 5
    ( ?????? ) BLOCKTYPE_ANIMBKGRND  6
    ( ?????? ) BLOCKTYPE_COMPSCRIPT3 7     // new CSCOMP script instead of SeeR
    (2.56, 22) BLOCKTYPE_PROPERTIES  8
    (2.70, 24) BLOCKTYPE_OBJECTSCRIPTNAMES 9
    ( ?????? ) BLOCKTYPE_EOF         0xff
    */

    /* room file versions history
     6:  new file layout using blocks
     7:  ???
     8:  final v1.14 release
     9:  intermediate v2 alpha releases
    10:  v2 alpha-7 release
    11:  final v2.00 release
    12:  v2.08, to add colour depth byte
    13:  v2.14, add walkarea light levels
    14:  v2.4, fixed so it saves walkable area 15
    15:  v2.41, supports NewInteraction
    16:  v2.5 - (what changed?)
    17:  v2.5 - just version change to force room re-compile for new charctr struct
    18:  v2.51 - vector scaling
    19:  v2.53 - interaction variables
    20:  v2.55 - shared palette backgrounds
    21:  v2.55 - regions (same realese?)
    22:  v2.61 - encrypt room messages (first seen in v2.56d) + added properties block
    23:  v2.62 - object flags
    24:  v2.7  - hotspot script names + added object script names block
    25:  v2.72 - game id embedded
    26:  v3.0 - new interaction format, and no script source
    27:  v3.0 - store Y of bottom of object, not top
    28:  v3.0.3 - remove hotspot name length limit
    29:  v3.0.3 - high-res coords for object x/y, edges and hotspot walk-to point
    30:  v3.4.0.4 - tint luminance for regions
    31:  v3.4.1.5 - removed room object and hotspot name length limits
    32:  v3.5.0 - 64-bit file offsets
    33:  v3.5.0.8 - deprecated room resolution, added mask resolution
    33:  v3.6.0 - extension blocks (no version bump?)
    */
    public int Version;
    public string Name;
    public int Width;
    public int Height;
    public int ResolutionType;
    public uint GameID;
    public byte[] Password;

    public AGSRoomState State;
    public AGSRoomBackground Background;
    public AGSRoomMarkup Markup;
    public AGSRoomEdges Edges;
    public AGSRoomScript Script;
    public AGSRoomProperties Properties;
    public AGSInteractions Interactions;
    public AGSMessage[] Messages;

    public AGSRoomDeprecated Deprecated;

    public Dictionary<string, string> Options;

    public AGSRoom()
      : this(string.Empty)
    {
    }

    // TODO(adm244): pass a room version into constructor
    public AGSRoom(string name)
    {
      Version = 29;
      Name = name;
      Width = 320;
      Height = 200;
      ResolutionType = 1;
      GameID = 0;
      Password = new byte[0];

      State = new AGSRoomState();
      Background = new AGSRoomBackground();
      Markup = new AGSRoomMarkup();
      Edges = new AGSRoomEdges();
      Script = new AGSRoomScript();
      Properties = new AGSRoomProperties(Markup);
      Interactions = new AGSInteractions();
      Messages = new AGSMessage[0];

      Deprecated = new AGSRoomDeprecated();

      Options = new Dictionary<string, string>();
    }

    public static AGSRoom ReadFromFile(string filepath)
    {
      AGSRoom room = new AGSRoom();

      using FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
      using BinaryReader reader = new BinaryReader(stream, Encoding.Latin1);

      room.ReadFromStream(reader);

      return room;
    }

    // FIXME(adm244): remove this
    public void ReadFromFileDeprecated(string filepath)
    {
      using (FileStream stream = new FileStream(filepath, FileMode.Open))
      {
        using (BinaryReader reader = new BinaryReader(stream, Encoding.Latin1))
        {
          ReadFromStream(reader);
        }
      }
    }

    public void ReadFromStream(BinaryReader reader)
    {
      Version = reader.ReadInt16();

      if (Version < 6)
        throw new NotSupportedException($"This room version is not supported, it's too old!\n\nVersion: {Version}");

      while (true)
      {
        ExtensionBlock.BlockType blockType = ExtensionBlock.ReadSingle(reader, ReadRoomExtensionBlock,
          ExtensionBlock.Options.Id8 | ExtensionBlock.Options.Size64);
        BlockType roomBlockType = (BlockType)blockType;

        if (roomBlockType == BlockType.EndOfFile)
          break;

        bool isExtensionBlock = Enum.IsDefined(blockType) && blockType >= 0;
        bool isRoomBlock = Enum.IsDefined(roomBlockType);

        if (isExtensionBlock)
          continue;

        if (!isRoomBlock)
          throw new InvalidDataException($"Unknown block type encountered: {blockType}");

        ReadRoomBlock(reader, Version, roomBlockType);
      }
    }

    public void WriteToFile(string filePath, int roomVersion)
    {
      //TODO(adm244): this entire room reading/writing code is a mess
      // and not only AGS is to blame here, consider "encapsulating" version specifics

      using (FileStream stream = new FileStream(filePath, FileMode.Create))
      {
        //FIXME(adm244): considering AGS now claims to support unicode, it's no longer valid
        // to use Latin1 encoding to read "unicode compatable" files
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Latin1))
        {
          writer.Write((UInt16)roomVersion);

          //NOTE(adm244): always write a MAIN block first, since many others depend on it
          WriteRoomBlock(writer, roomVersion, BlockType.Main);

          if (!string.IsNullOrEmpty(Script.SourceCode))
            WriteRoomBlock(writer, roomVersion, BlockType.ScriptSource);

          //FIXME(adm244): write only if it's not null
          WriteRoomBlock(writer, roomVersion, BlockType.ObjectNames);

          if (Background.Frames.Count > 1)
            WriteRoomBlock(writer, roomVersion, BlockType.BackgroundFrames);

          WriteRoomBlock(writer, roomVersion, BlockType.ScriptSCOM3);

          if (roomVersion >= 22)
            WriteRoomBlock(writer, roomVersion, BlockType.Properties);

          //FIXME(adm244): write only if it's not null
          if (roomVersion >= 24)
            WriteRoomBlock(writer, roomVersion, BlockType.ObjectScriptNames);

          if (roomVersion >= 33)
          {
            if (Options.Count > 0)
              ExtensionBlock.WriteSingle(writer, "ext_sopts", WriteRoomExtensionBlock,
                ExtensionBlock.Options.Id8 | ExtensionBlock.Options.Size64);
          }

          WriteRoomBlock(writer, roomVersion, BlockType.EndOfFile);
        }
      }
    }

    private Int64 ReadRoomBlockLength(BinaryReader reader, int roomVersion)
    {
      if (roomVersion < 32)
        return reader.ReadInt32();

      return reader.ReadInt64();
    }

    private void WriteRoomBlockLength(BinaryWriter writer, int roomVersion, long length)
    {
      if (roomVersion < 32)
        writer.Write((UInt32)length);
      else
        writer.Write((Int64)length);
    }

    private bool ReadRoomExtensionBlock(BinaryReader reader, string id, long size)
    {
      switch (id)
      {
        case "ext_sopts":
          return ReadOptionsExtensionBlock(reader);

        default:
          Debug.Assert(false, $"Room extension block '{id}' is not supported!");
          return false;
      }
    }

    private bool WriteRoomExtensionBlock(BinaryWriter writer, string id)
    {
      switch (id)
      {
        case "ext_sopts":
          return WriteOptionsExtensionBlock(writer);

        default:
          throw new NotImplementedException($"Room extension block '{id}' is not implemented!");
      }
    }

    private bool ReadOptionsExtensionBlock(BinaryReader reader)
    {
      int count = reader.ReadInt32();

      for (int i = 0; i < count; ++i)
      {
        string key = reader.ReadPrefixedString32();
        string value = reader.ReadPrefixedString32();

        Options.Add(key, value);
      }

      return true;
    }

    private bool WriteOptionsExtensionBlock(BinaryWriter writer)
    {
      writer.Write((Int32)Options.Count);

      foreach (var option in Options)
      {
        writer.WritePrefixedString32(option.Key);
        writer.WritePrefixedString32(option.Value);
      }

      return true;
    }

    private void ReadRoomBlock(BinaryReader reader, int roomVersion, BlockType type)
    {
      //TODO(adm244): unused for now, maybe we should check it after reading a block
      // OR redesign reading\writing so blocks are read into memory and then parsed?
      Int64 length = ReadRoomBlockLength(reader, roomVersion);

      switch (type)
      {
        case BlockType.Main:
          ReadRoomMainBlock(reader, roomVersion);
          break;
        case BlockType.ScriptSource:
          Script.ReadSourceBlock(reader, roomVersion);
          break;
        case BlockType.ObjectNames:
          Markup.ReadObjectNamesBlock(reader, roomVersion);
          break;
        case BlockType.BackgroundFrames:
          Background.ReadBlock(reader, roomVersion);
          break;
        case BlockType.ScriptSCOM3:
          Script.ReadSCOM3Block(reader, roomVersion);
          break;
        case BlockType.Properties:
          Properties.ReadBlock(reader, roomVersion);
          break;
        case BlockType.ObjectScriptNames:
          Markup.ReadObjectScriptNamesBlock(reader, roomVersion);
          break;

        default:
          throw new NotImplementedException("Room block is not implemented");
      }
    }

    private void WriteRoomBlock(BinaryWriter writer, int roomVersion, BlockType type)
    {
      writer.Write((byte)type);
      if (type == BlockType.EndOfFile)
        return;

      long blockStartPreSize = writer.BaseStream.Position;

      //NOTE(adm244): a placeholder for an actual value
      WriteRoomBlockLength(writer, roomVersion, unchecked((long)0xDEADBEEF_DEADBEEF));

      long blockStart = writer.BaseStream.Position;

      switch (type)
      {
        case BlockType.Main:
          WriteRoomMainBlock(writer, roomVersion);
          break;
        case BlockType.ScriptSource:
          Script.WriteSourceBlock(writer, roomVersion);
          break;
        case BlockType.ObjectNames:
          Markup.WriteObjectNamesBlock(writer, roomVersion);
          break;
        case BlockType.BackgroundFrames:
          Background.WriteBlock(writer, roomVersion);
          break;
        case BlockType.ScriptSCOM3:
          Script.WriteSCOM3Block(writer, roomVersion);
          break;
        case BlockType.Properties:
          Properties.WriteBlock(writer, roomVersion);
          break;
        case BlockType.ObjectScriptNames:
          Markup.WriteObjectScriptNamesBlock(writer, roomVersion);
          break;

        default:
          throw new NotImplementedException("Room block is not implemented!");
      }

      long blockEnd = writer.BaseStream.Position;
      long blockLength = blockEnd - blockStart;
      Debug.Assert(blockLength < Int64.MaxValue);

      writer.BaseStream.Seek(blockStartPreSize, SeekOrigin.Begin);
      WriteRoomBlockLength(writer, roomVersion, blockLength);
      writer.BaseStream.Seek(blockEnd, SeekOrigin.Begin);
    }

    private void ReadRoomMainBlock(BinaryReader reader, int roomVersion)
    {
      if (roomVersion >= 12) // v2.08+
        Background.BytesPerPixel = reader.ReadInt32();
      else
        Background.BytesPerPixel = 1;

      ReadWalkbehindAreasBaselines(reader, roomVersion);
      ReadHotspots(reader, roomVersion);
      ReadPolypoints(reader, roomVersion);
      ReadEdges(reader, roomVersion);
      ReadObjects(reader, roomVersion);
      ReadInteractions(reader, roomVersion);
      ReadObjectsExtraAndRoomResolution(reader, roomVersion);
      ReadWalkableAreasInfo(reader, roomVersion);
      ReadRoomSettings(reader, roomVersion);
      ReadMessagesCountAndGameID(reader, roomVersion);
      ReadRoomMessages(reader, roomVersion);

      if (roomVersion >= 6) // ???
        ReadLegacyRoomAnimations(reader, roomVersion);

      if ((roomVersion >= 4) && (roomVersion < 16)) // ???, 2.5
      {
        ReadLegacyScriptConfig(reader, roomVersion);
        ReadLegacyGraphicalScripts(reader, roomVersion);
      }

      ReadAreasLightLevels(reader, roomVersion);
      ReadRoomBitmaps(reader, roomVersion);
    }

    private void WriteRoomMainBlock(BinaryWriter writer, int roomVersion)
    {
      if (roomVersion >= 12) // 2.08+
        writer.Write((Int32)Background.BytesPerPixel);

      WriteWalkbehindAreasBaselines(writer, roomVersion);
      WriteRoomHotspots(writer, roomVersion);
      WriteRoomPolypoints(writer, roomVersion);
      WriteEdges(writer, roomVersion);
      WriteObjects(writer, roomVersion);
      WriteInteractions(writer, roomVersion);
      WriteObjectsExtraAndRoomResolution(writer, roomVersion);
      WriteWalkableAreasInfo(writer, roomVersion);
      WriteRoomSettings(writer, roomVersion);
      WriteMessagesCountAndGameID(writer, roomVersion);
      WriteRoomMessages(writer, roomVersion);

      if (roomVersion >= 6)
        WriteLegacyRoomAnimations(writer, roomVersion);

      if ((roomVersion >= 4) && (roomVersion < 16)) // ???, 2.5
      {
        WriteLegacyScriptConfig(writer, roomVersion);
        WriteLegacyGraphicalScripts(writer, roomVersion);
      }

      WriteAreasLightLevels(writer, roomVersion);
      WriteRoomBitmaps(writer, roomVersion);
    }

    private void ReadWalkbehindAreasBaselines(BinaryReader reader, int roomVersion)
    {
      Int16 count = reader.ReadInt16();

      Markup.WalkbehindAreas = new AGSWalkbehindArea[count];
      for (int i = 0; i < Markup.WalkbehindAreas.Length; ++i)
      {
        Markup.WalkbehindAreas[i] = new AGSWalkbehindArea();
        Markup.WalkbehindAreas[i].Baseline = reader.ReadInt16();
      }
    }

    private void WriteWalkbehindAreasBaselines(BinaryWriter writer, int roomVersion)
    {
      writer.Write((Int16)Markup.WalkbehindAreas.Length);

      for (int i = 0; i < Markup.WalkbehindAreas.Length; ++i)
        writer.Write((Int16)Markup.WalkbehindAreas[i].Baseline);
    }

    private void ReadHotspots(BinaryReader reader, int roomVersion)
    {
      Int32 count = reader.ReadInt32();

      if (Version < 15) // 2.41 (never released?)
      {
        for (int i = 0; i < Deprecated.HotspotConditions.Length; ++i)
          Deprecated.HotspotConditions[i] = AGSEventBlock.ReadFromStream(reader);

        for (int i = 0; i < Deprecated.ObjectConditions.Length; ++i)
          Deprecated.ObjectConditions[i] = AGSEventBlock.ReadFromStream(reader);

        Deprecated.MiscConditions = AGSEventBlock.ReadFromStream(reader);
      }

      Markup.Hotspots = new AGSHotspot[count];
      for (int i = 0; i < Markup.Hotspots.Length; ++i)
      {
        Markup.Hotspots[i] = new AGSHotspot();
        Markup.Hotspots[i].WalkTo.X = reader.ReadInt16();
        Markup.Hotspots[i].WalkTo.Y = reader.ReadInt16();
      }

      // read hotspots names
      for (int i = 0; i < Markup.Hotspots.Length; ++i)
      {
        //NOTE(adm244): can't really decide which one to use, eh?
        // How about encryption again? Jibzle it! Oh, yeah, it's "open-source" now...
        if (roomVersion >= 31) // 3.4.1.5
          Markup.Hotspots[i].Name = reader.ReadPrefixedString32();
        else if (roomVersion >= 28) // ???
          Markup.Hotspots[i].Name = reader.ReadCString();
        else
          Markup.Hotspots[i].Name = reader.ReadFixedCString(30);
      }

      // read hotspots scriptnames
      if (roomVersion >= 24) // ???
      {
        for (int i = 0; i < Markup.Hotspots.Length; ++i)
        {
          if (roomVersion >= 31) // 3.4.1.5
            Markup.Hotspots[i].ScriptName = reader.ReadPrefixedString32();
          else
            Markup.Hotspots[i].ScriptName = reader.ReadFixedCString(20);
        }
      }
    }

    private void WriteRoomHotspots(BinaryWriter writer, int roomVersion)
    {
      writer.Write((Int32)Markup.Hotspots.Length);

      if (Version < 15) // 2.41 (never released?)
      {
        for (int i = 0; i < Deprecated.HotspotConditions.Length; ++i)
          Deprecated.HotspotConditions[i].WriteToStream(writer);

        for (int i = 0; i < Deprecated.ObjectConditions.Length; ++i)
          Deprecated.ObjectConditions[i].WriteToStream(writer);

        Deprecated.MiscConditions.WriteToStream(writer);
      }

      for (int i = 0; i < Markup.Hotspots.Length; ++i)
      {
        writer.Write((Int16)Markup.Hotspots[i].WalkTo.X);
        writer.Write((Int16)Markup.Hotspots[i].WalkTo.Y);
      }

      // write hotspots names
      for (int i = 0; i < Markup.Hotspots.Length; ++i)
      {
        if (roomVersion >= 31) // 3.4.1.5
          writer.WritePrefixedString32(Markup.Hotspots[i].Name);
        else if (roomVersion >= 28) // ???
          writer.WriteCString(Markup.Hotspots[i].Name);
        else
          writer.WriteFixedString(Markup.Hotspots[i].Name, 30);
      }

      // write hotspots scriptnames
      if (roomVersion >= 24) // ???
      {
        for (int i = 0; i < Markup.Hotspots.Length; ++i)
        {
          if (roomVersion >= 31)
            writer.WritePrefixedString32(Markup.Hotspots[i].ScriptName);
          else
            writer.WriteFixedString(Markup.Hotspots[i].ScriptName, 20);
        }
      }
    }

    private void ReadPolypoints(BinaryReader reader, int roomVersion)
    {
      Int32 count = reader.ReadInt32();

      //TODO(adm244): implement room polypoints reader
      if (count > 0)
        throw new NotImplementedException("CRM: Polypoints reader is not implemented.");
    }

    private void WriteRoomPolypoints(BinaryWriter writer, int roomVersion)
    {
      //TODO(adm244): implement room polypoints writer
      writer.Write((Int32)0x0);
    }

    private void ReadEdges(BinaryReader reader, int roomVersion)
    {
      Edges.Top = reader.ReadInt16();
      Edges.Bottom = reader.ReadInt16();
      Edges.Left = reader.ReadInt16();
      Edges.Right = reader.ReadInt16();
    }

    private void WriteEdges(BinaryWriter writer, int roomVersion)
    {
      writer.Write((Int16)Edges.Top);
      writer.Write((Int16)Edges.Bottom);
      writer.Write((Int16)Edges.Left);
      writer.Write((Int16)Edges.Right);
    }

    private void ReadObjects(BinaryReader reader, int roomVersion)
    {
      Int16 count = reader.ReadInt16();

      Markup.Objects = new AGSObject[count];
      for (int i = 0; i < count; ++i)
      {
        Markup.Objects[i] = new AGSObject();
        Markup.Objects[i].Sprite = reader.ReadInt16();
        Markup.Objects[i].Position.X = reader.ReadInt16();
        Markup.Objects[i].Position.Y = reader.ReadInt16();
        Markup.Objects[i].Room = reader.ReadInt16();
        Markup.Objects[i].Visible = Convert.ToBoolean(reader.ReadInt16());
      }
    }

    private void WriteObjects(BinaryWriter writer, int roomVersion)
    {
      writer.Write((Int16)Markup.Objects.Length);

      for (int i = 0; i < Markup.Objects.Length; ++i)
      {
        writer.Write((Int16)Markup.Objects[i].Sprite);
        writer.Write((Int16)Markup.Objects[i].Position.X);
        writer.Write((Int16)Markup.Objects[i].Position.Y);
        writer.Write((Int16)Markup.Objects[i].Room);
        writer.Write((Int16)Convert.ToInt16(Markup.Objects[i].Visible));
      }
    }

    private void ReadInteractionsOld(BinaryReader reader, int roomVersion)
    {
      for (int i = 0; i < Markup.Hotspots.Length; ++i)
      {
        Markup.Hotspots[i].Interactions.Interaction = new AGSInteraction();
        Markup.Hotspots[i].Interactions.Interaction.LoadFromStream(reader);
      }

      for (int i = 0; i < Markup.Objects.Length; ++i)
      {
        Markup.Objects[i].Interactions.Interaction = new AGSInteraction();
        Markup.Objects[i].Interactions.Interaction.LoadFromStream(reader);
      }

      Interactions.Interaction = new AGSInteraction();
      Interactions.Interaction.LoadFromStream(reader);
    }

    private void WriteInteractionsOld(BinaryWriter writer, int roomVersion)
    {
      for (int i = 0; i < Markup.Hotspots.Length; ++i)
        Markup.Hotspots[i].Interactions.Interaction.WriteToStream(writer);

      for (int i = 0; i < Markup.Objects.Length; ++i)
        Markup.Objects[i].Interactions.Interaction.WriteToStream(writer);

      Interactions.Interaction.WriteToStream(writer);
    }

    private void ReadRegionInteractionsOld(BinaryReader reader, int roomVersion)
    {
      for (int i = 0; i < Markup.Regions.Length; ++i)
      {
        Markup.Regions[i].Interactions.Interaction = new AGSInteraction();
        Markup.Regions[i].Interactions.Interaction.LoadFromStream(reader);
      }
    }

    private void WriteRegionInteractionsOld(BinaryWriter writer, int roomVersion)
    {
      for (int i = 0; i < Markup.Regions.Length; ++i)
        Markup.Regions[i].Interactions.Interaction.WriteToStream(writer);
    }

    private void ReadInteractionScripts(BinaryReader reader, int roomVersion)
    {
      // parse room events
      Interactions.Script.LoadFromStream(reader);

      // parse hotspot events
      for (int i = 0; i < Markup.Hotspots.Length; ++i)
        Markup.Hotspots[i].Interactions.Script.LoadFromStream(reader);

      // parse object events
      for (int i = 0; i < Markup.Objects.Length; ++i)
        Markup.Objects[i].Interactions.Script.LoadFromStream(reader);

      // parse region events
      for (int i = 0; i < Markup.Regions.Length; ++i)
        Markup.Regions[i].Interactions.Script.LoadFromStream(reader);
    }

    private void WriteInteractionScripts(BinaryWriter writer, int roomVersion)
    {
      // write room events
      Interactions.Script.WriteToStream(writer);

      // write hotspot events
      for (int i = 0; i < Markup.Hotspots.Length; ++i)
        Markup.Hotspots[i].Interactions.Script.WriteToStream(writer);

      // write object events
      for (int i = 0; i < Markup.Objects.Length; ++i)
        Markup.Objects[i].Interactions.Script.WriteToStream(writer);

      // write region events
      for (int i = 0; i < Markup.Regions.Length; ++i)
        Markup.Regions[i].Interactions.Script.WriteToStream(writer);
    }

    private void ReadInteractions(BinaryReader reader, int roomVersion)
    {
      if (roomVersion >= 19) // 2.53
      {
        Int32 interactionVariablesCount = reader.ReadInt32();

        Interactions.InteractionsLegacy = new AGSInteractionLegacy[interactionVariablesCount];
        for (int i = 0; i < interactionVariablesCount; ++i)
        {
          Interactions.InteractionsLegacy[i] = new AGSInteractionLegacy();
          Interactions.InteractionsLegacy[i].ReadFromStream(reader);
        }
      }

      if (roomVersion >= 15) // 2.41 (never released?)
      {
        if (roomVersion < 26) // ???
          ReadInteractionsOld(reader, roomVersion);

        if (roomVersion >= 21) // ???
        {
          Int32 regionsCount = reader.ReadInt32();
          Markup.Regions = new AGSRegion[regionsCount];
          for (int i = 0; i < Markup.Regions.Length; ++i)
            Markup.Regions[i] = new AGSRegion();

          if (roomVersion < 26) // ???
            ReadRegionInteractionsOld(reader, roomVersion);
        }

        if (roomVersion >= 26) // ???
          ReadInteractionScripts(reader, roomVersion);
      }
    }

    private void WriteInteractions(BinaryWriter writer, int roomVersion)
    {
      if (roomVersion >= 19) // ???
      {
        writer.Write((Int32)Interactions.InteractionsLegacy.Length);

        for (int i = 0; i < Interactions.InteractionsLegacy.Length; ++i)
        {
          Interactions.InteractionsLegacy[i].WriteToStream(writer);
        }
      }

      if (roomVersion >= 15) // ???
      {
        if (roomVersion < 26) // ???
          WriteInteractionsOld(writer, roomVersion);

        if (roomVersion >= 21) // ???
        {
          writer.Write((Int32)Markup.Regions.Length);

          if (roomVersion < 26) // ???
            WriteRegionInteractionsOld(writer, roomVersion);
        }

        if (roomVersion >= 26) // ???
          WriteInteractionScripts(writer, roomVersion);
      }
    }

    private void ReadObjectsExtraAndRoomResolution(BinaryReader reader, int roomVersion)
    {
      if (roomVersion >= 9) // ???
      {
        // read objects baselines
        for (int i = 0; i < Markup.Objects.Length; ++i)
          Markup.Objects[i].Baseline = reader.ReadInt32();

        // read room dimensions
        Width = reader.ReadInt16();
        Height = reader.ReadInt16();
      }

      if (roomVersion >= 23) // ???
      {
        // read objects flags
        for (int i = 0; i < Markup.Objects.Length; ++i)
          Markup.Objects[i].Flags = reader.ReadInt16();
      }

      if (roomVersion >= 11) // ???
        ResolutionType = reader.ReadInt16();
    }

    private void WriteObjectsExtraAndRoomResolution(BinaryWriter writer, int roomVersion)
    {
      if (roomVersion >= 9) // ???
      {
        // write objects baselines
        for (int i = 0; i < Markup.Objects.Length; ++i)
          writer.Write((Int32)Markup.Objects[i].Baseline);

        // write room dimensions
        writer.Write((Int16)Width);
        writer.Write((Int16)Height);
      }

      if (roomVersion >= 23) // ???
      {
        // write objects flags
        for (int i = 0; i < Markup.Objects.Length; ++i)
          writer.Write((Int16)Markup.Objects[i].Flags);
      }

      if (roomVersion >= 11) // ???
        writer.Write((Int16)ResolutionType);
    }

    private void ReadWalkableAreasInfo(BinaryReader reader, int roomVersion)
    {
      Int32 count = 15;
      if (roomVersion >= 14) // 2.4
        count = reader.ReadInt32();

      Markup.WalkableAreas = new AGSWalkableArea[count];
      for (int i = 0; i < Markup.WalkableAreas.Length; ++i)
        Markup.WalkableAreas[i] = new AGSWalkableArea();

      if (roomVersion >= 10) // ???
      {
        for (int i = 0; i < Markup.WalkableAreas.Length; ++i)
          Markup.WalkableAreas[i].ScaleFar = reader.ReadInt16();
      }

      if (roomVersion >= 13) // ???
      {
        for (int i = 0; i < Markup.WalkableAreas.Length; ++i)
          Markup.WalkableAreas[i].Light = reader.ReadInt16();
      }

      if (roomVersion >= 18) // ???
      {
        for (int i = 0; i < Markup.WalkableAreas.Length; ++i)
          Markup.WalkableAreas[i].ScaleNear = reader.ReadInt16();

        for (int i = 0; i < Markup.WalkableAreas.Length; ++i)
          Markup.WalkableAreas[i].TopY = reader.ReadInt16();

        for (int i = 0; i < Markup.WalkableAreas.Length; ++i)
          Markup.WalkableAreas[i].BottomY = reader.ReadInt16();
      }
    }

    private void WriteWalkableAreasInfo(BinaryWriter writer, int roomVersion)
    {
      if (roomVersion >= 14) // ???
        writer.Write((Int32)Markup.WalkableAreas.Length);

      if (roomVersion >= 10) // ???
      {
        for (int i = 0; i < Markup.WalkableAreas.Length; ++i)
          writer.Write((Int16)Markup.WalkableAreas[i].ScaleFar);
      }

      if (roomVersion >= 13) // ???
      {
        for (int i = 0; i < Markup.WalkableAreas.Length; ++i)
          writer.Write((Int16)Markup.WalkableAreas[i].Light);
      }

      if (roomVersion >= 18) // ???
      {
        for (int i = 0; i < Markup.WalkableAreas.Length; ++i)
          writer.Write((Int16)Markup.WalkableAreas[i].ScaleNear);

        for (int i = 0; i < Markup.WalkableAreas.Length; ++i)
          writer.Write((Int16)Markup.WalkableAreas[i].TopY);

        for (int i = 0; i < Markup.WalkableAreas.Length; ++i)
          writer.Write((Int16)Markup.WalkableAreas[i].BottomY);
      }
    }

    private byte[] DecryptRoomPassword(byte[] bufferEncrypted, int roomVersion)
    {
      if (roomVersion < 9) // ???
      {
        byte[] bufferDecrypted = new byte[bufferEncrypted.Length];

        for (int i = 0; i < bufferEncrypted.Length; ++i)
          bufferDecrypted[i] = (byte)(bufferEncrypted[i] - (byte)60);

        return bufferDecrypted;
      }

      //NOTE(adm244): not a bug, it decrypts by encrypting
      return AGSEncryption.EncryptAvisBuffer(bufferEncrypted);
    }

    private byte[] EncryptRoomPassword(byte[] buffer, int roomVersion)
    {
      if (roomVersion < 9)
      {
        byte[] bufferEncrypted = new byte[buffer.Length];
        
        for (int i = 0; i < bufferEncrypted.Length; ++i)
          bufferEncrypted[i] = (byte)(buffer[i] - (byte)60);
        
        return bufferEncrypted;
      }

      //NOTE(adm244): not a bug, it encrypts by decrypting
      return AGSEncryption.DecryptAvisBuffer(buffer);
    }

    private void ReadRoomSettings(BinaryReader reader, int roomVersion)
    {
      Password = reader.ReadBytes(11);
      Password = DecryptRoomPassword(Password, roomVersion);

      State.StartupMusicID = reader.ReadByte();
      State.IsSaveLoadDisabled = Convert.ToBoolean(reader.ReadByte());
      State.IsPlayerInvisible = Convert.ToBoolean(reader.ReadByte());
      State.PlayerViewID = reader.ReadByte();
      State.MusicVolume = reader.ReadByte();

      reader.BaseStream.Seek(5, SeekOrigin.Current);
    }

    private void WriteRoomSettings(BinaryWriter writer, int roomVersion)
    {
      byte[] password_encrypted = EncryptRoomPassword(Password, roomVersion);
      writer.Write((byte[])password_encrypted);

      writer.Write((byte)State.StartupMusicID);
      writer.Write((byte)Convert.ToByte(State.IsSaveLoadDisabled));
      writer.Write((byte)Convert.ToByte(State.IsPlayerInvisible));
      writer.Write((byte)State.PlayerViewID);
      writer.Write((byte)State.MusicVolume);
      
      writer.Write((byte[])new byte[5]);
    }

    private void ReadMessagesCountAndGameID(BinaryReader reader, int roomVersion)
    {
      Int16 count = reader.ReadInt16();

      Messages = new AGSMessage[count];
      for (int i = 0; i < Messages.Length; ++i)
        Messages[i] = new AGSMessage();

      if (roomVersion >= 25) // ???
        GameID = reader.ReadUInt32();
    }

    private void WriteMessagesCountAndGameID(BinaryWriter writer, int roomVersion)
    {
      writer.Write((Int16)Messages.Length);

      if (roomVersion >= 25) // ???
        writer.Write((UInt32)GameID);
    }

    private void ReadRoomMessages(BinaryReader reader, int roomVersion)
    {
      if (roomVersion >= 3) // ???
      {
        // read messages flags
        for (int i = 0; i < Messages.Length; ++i)
        {
          Messages[i].DisplayAs = reader.ReadByte();
          Messages[i].Flags = reader.ReadByte();
        }
      }

      // read messages text
      for (int i = 0; i < Messages.Length; ++i)
      {
        if (roomVersion >= 22) // ???
          Messages[i].Text = reader.ReadEncryptedCString();
        else
          Messages[i].Text = reader.ReadCString(2999);
      }
    }

    private void WriteRoomMessages(BinaryWriter writer, int roomVersion)
    {
      if (roomVersion >= 3) // ???
      {
        // write messages flags
        for (int i = 0; i < Messages.Length; ++i)
        {
          writer.Write((byte)Messages[i].DisplayAs);
          writer.Write((byte)Messages[i].Flags);
        }
      }

      // write messages text
      for (int i = 0; i < Messages.Length; ++i)
      {
        if (roomVersion >= 22) // ???
          writer.WriteEncryptedCString(Messages[i].Text);
        else
          writer.WriteCString(Messages[i].Text, 2999);
      }
    }

    private void ReadLegacyRoomAnimations(BinaryReader reader, int roomVersion)
    {
      Int16 count = reader.ReadInt16();

      //TODO(adm244): implement legacy room animations reader
      if (count > 0)
        throw new NotImplementedException("CRM: Legacy room animations reader is not implemented.");
    }

    private void WriteLegacyRoomAnimations(BinaryWriter writer, int roomVersion)
    {
      //TODO(adm244): implement legacy room animations writer
      writer.Write((Int16)0x0);
    }

    private void ReadLegacyScriptConfig(BinaryReader reader, int roomVersion)
    {
      int version = reader.ReadInt32();
      if (version != 1)
        throw new NotSupportedException($"Unknown script configuration version detected: {version}");

      int count = reader.ReadInt32();

      Deprecated.VariableNames = new string[count];
      for (int i = 0; i < Deprecated.VariableNames.Length; ++i)
        Deprecated.VariableNames[i] = reader.ReadPrefixedString8();
    }

    private void WriteLegacyScriptConfig(BinaryWriter writer, int roomVersion)
    {
      writer.Write((Int32)1);
      writer.Write((Int32)Deprecated.VariableNames.Length);

      for (int i = 0; i < Deprecated.VariableNames.Length; ++i)
        writer.WritePrefixedString8(Deprecated.VariableNames[i]);
    }

    private void ReadLegacyGraphicalScripts(BinaryReader reader, int roomVersion)
    {
      // NOTE(adm244): 2.31 has 20 script blocks limit
      for (int i = 0; i < 20 && !reader.EOF(); ++i)
      {
        int id = reader.ReadInt32();
        if (id == -1)
          break;

        int size = reader.ReadInt32();
        long start = reader.BaseStream.Position;

        AGSGraphicalScript script = AGSGraphicalScript.ReadFromStream(reader, id);
        Deprecated.GraphicalScripts.Add(script);

        long end = reader.BaseStream.Position;
        if ((end - start) != size)
          throw new InvalidDataException($"Invalid graphical script size.\n\nGot: {end - start}\nExpected: {size}");
      }
    }

    private void WriteLegacyGraphicalScripts(BinaryWriter writer, int roomVersion)
    {
      for (int i = 0; i < Deprecated.GraphicalScripts.Count; ++i)
      {
        writer.Write((Int32)Deprecated.GraphicalScripts[i].Id);

        long sizePosition = writer.BaseStream.Position;
        writer.Write((UInt32)0xDEADBEEF);

        Deprecated.GraphicalScripts[i].WriteToStream(writer);

        writer.FixInt32(sizePosition);
      }

      writer.Write((Int32)(-1));
    }

    private void ReadAreasLightLevels(BinaryReader reader, int roomVersion)
    {
      if (roomVersion >= 8) // ???
      {
        // read walkable areas light level (unused?)
        for (int i = 0; i < Markup.WalkableAreas.Length; ++i)
          Markup.WalkableAreas[i].Light = reader.ReadInt16();

        // FIXME(adm244): dirty quick fix for walkies count debacle
        if (roomVersion < 14)
          reader.ReadInt16();
      }

      if (roomVersion >= 21) // ???
      {
        // read regions light level
        for (int i = 0; i < Markup.Regions.Length; ++i)
          Markup.Regions[i].Light = reader.ReadInt16();

        // read regions tint colors
        for (int i = 0; i < Markup.Regions.Length; ++i)
          Markup.Regions[i].Tint = reader.ReadInt32();
      }
    }

    private void WriteAreasLightLevels(BinaryWriter writer, int roomVersion)
    {
      if (roomVersion >= 8) // ???
      {
        // write walkable areas light level (unused)
        for (int i = 0; i < Markup.WalkableAreas.Length; ++i)
          writer.Write((Int16)Markup.WalkableAreas[i].Light);

        // FIXME(adm244): dirty quick fix for walkies count debacle
        if (roomVersion < 14)
          writer.Write((Int16)0);
      }

      if (roomVersion >= 21) // ???
      {
        // write regions light level
        for (int i = 0; i < Markup.Regions.Length; ++i)
          writer.Write((Int16)Markup.Regions[i].Light);

        // write regions tint colors
        for (int i = 0; i < Markup.Regions.Length; ++i)
          writer.Write((Int32)Markup.Regions[i].Tint);
      }
    }

    private void ReadRoomBitmaps(BinaryReader reader, int roomVersion)
    {
      if (roomVersion >= 5) // ???
        Background.Frames.Add(AGSGraphics.ReadLZ77Image(reader, Background.BytesPerPixel));
      else
        Background.Frames.Add(AGSGraphics.ReadAllegroImage(reader));

      Background.RegionsMask = AGSGraphics.ReadAllegroImage(reader);
      Background.WalkableAreasMask = AGSGraphics.ReadAllegroImage(reader);
      Background.WalkbehindAreasMask = AGSGraphics.ReadAllegroImage(reader);
      Background.HotspotsMask = AGSGraphics.ReadAllegroImage(reader);
    }

    private void WriteRoomBitmaps(BinaryWriter writer, int roomVersion)
    {
      if (roomVersion >= 5)
        AGSGraphics.WriteLZ77Image(writer, Background.MainBackground, Background.BytesPerPixel);
      else
        AGSGraphics.WriteAllegroImage(writer, Background.MainBackground);

      AGSGraphics.WriteAllegroImage(writer, Background.RegionsMask);
      AGSGraphics.WriteAllegroImage(writer, Background.WalkableAreasMask);
      AGSGraphics.WriteAllegroImage(writer, Background.WalkbehindAreasMask);
      AGSGraphics.WriteAllegroImage(writer, Background.HotspotsMask);
    }

    private enum BlockType
    {
      Main = 0x01,
      ScriptSource = 0x02,
      ObjectNames = 0x05,
      BackgroundFrames = 0x06,
      ScriptSCOM3 = 0x07,
      Properties = 0x08,
      ObjectScriptNames = 0x09,

      EndOfFile = 0xFF
    }
  }
}
