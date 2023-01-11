using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using AGSUnpacker.Lib.Game.View;
using AGSUnpacker.Lib.Shared;
using AGSUnpacker.Lib.Shared.FormatExtensions;
using AGSUnpacker.Lib.Shared.Interaction;
using AGSUnpacker.Shared.Extensions;
using AGSUnpacker.Shared.Utils.Encryption;

namespace AGSUnpacker.Lib.Game
{
  public struct AGSRoomDebugInfo
  {
    public Int32 id;
    public string name;
  }

  public class AGSGameData
  {
    private static readonly string GameDataSignature = "Adventure Creator Game File v2";
    private static readonly UInt32 GUI_SIGNATURE = 0xCAFEBEEF;

    private static readonly int LIMIT_MAX_GLOBAL_MESSAGES = 500;

    public int Version;
    public string VersionEngine;
    public string[] Capabilities;

    public List<string> ExtensionBlocks;

    public AGSGameSetupStruct setup;
    public char[] save_guid;
    public char[] save_extension;
    public char[] save_folder;
    public AGSFont[] fonts;
    public int[] font_outlines_thickness;
    public int[] font_outlines_style;
    public byte[] sprite_flags;
    public AGSInventoryItem[] inventoryItems;
    public AGSCursorInfo[] cursors;
    public AGSDictionary dictionary;
    public AGSScript globalScript;
    public AGSScript dialogScript;
    public AGSScript[] scriptModules;
    public AGSView[] views;
    public AGSCharacter[] characters;
    public string[] LipSyncFrameLetters;
    public string[] globalMessages;
    public AGSDialog[] dialogs;
    public AGSAudioStorage audioStorage;
    public AGSCustomPropertiesSchema customPropertiesSchema;
    public AGSRoomDebugInfo[] roomsDebugInfo;
    public AGSInteractionVariable[] globalvars;

    public AGSView272[] views272;
    public List<string> oldDialogStrings;

    public int GUIVersion;
    public AGSGUI[] guis;
    public AGSGUIButton[] buttons;
    public AGSGUILabel[] labels;
    public AGSGUIInventoryWindow[] inventoryWindows;
    public AGSGUISlider[] sliders;
    public AGSGUITextBox[] textboxes;
    public AGSGUIListBox[] listboxes;

    public int PluginVersion;
    public AGSPluginInfo[] Plugins;

    public AGSGameData()
    {
      Version = 0;
      VersionEngine = string.Empty;
      Capabilities = Array.Empty<string>();

      ExtensionBlocks = new List<string>();

      setup = new AGSGameSetupStruct();
      save_guid = new char[0];
      save_extension = new char[0];
      save_folder = new char[0];
      fonts = Array.Empty<AGSFont>();
      font_outlines_thickness = new int[0];
      font_outlines_style = new int[0];
      sprite_flags = new byte[0];
      inventoryItems = new AGSInventoryItem[0];
      cursors = new AGSCursorInfo[0];
      dictionary = new AGSDictionary();
      globalScript = new AGSScript();
      dialogScript = new AGSScript();
      scriptModules = new AGSScript[0];
      views = new AGSView[0];
      characters = new AGSCharacter[0];
      LipSyncFrameLetters = Array.Empty<string>();
      globalMessages = new string[0];
      dialogs = new AGSDialog[0];
      audioStorage = new AGSAudioStorage();
      customPropertiesSchema = new AGSCustomPropertiesSchema();
      roomsDebugInfo = new AGSRoomDebugInfo[0];
      globalvars = new AGSInteractionVariable[0];

      views272 = new AGSView272[0];
      oldDialogStrings = new List<string>();

      GUIVersion = 0;
      guis = new AGSGUI[0];
      buttons = new AGSGUIButton[0];
      labels = new AGSGUILabel[0];
      inventoryWindows = new AGSGUIInventoryWindow[0];
      sliders = new AGSGUISlider[0];
      textboxes = new AGSGUITextBox[0];
      listboxes = new AGSGUIListBox[0];

      PluginVersion = 0;
      Plugins = Array.Empty<AGSPluginInfo>();
    }

    public static AGSGameData ReadFromFile(string filepath)
    {
      AGSGameData gameData = new AGSGameData();

      using FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
      using BinaryReader reader = new BinaryReader(stream, Encoding.Latin1);

      gameData.LoadFromStream(reader);

      return gameData;
    }

    public void WriteToFile(string filepath)
    {
      using FileStream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write);
      using BinaryWriter writer = new BinaryWriter(stream, Encoding.Latin1);

      WriteToStream(writer);
    }

    // FIXME(adm244): remove this
    public void LoadFromFile(string filepath)
    {
      using (FileStream fs = new FileStream(filepath, FileMode.Open))
      {
        using (BinaryReader r = new BinaryReader(fs, Encoding.Latin1))
        {
          LoadFromStream(r);
        }
      }
    }

    // FIXME(adm244): this function can't be trusted
    public void LoadFromStream(BinaryReader r)
    {
      string signature = r.ReadFixedString(GameDataSignature.Length);
      Debug.Assert(GameDataSignature == signature);

      Version = r.ReadInt32();
      VersionEngine = r.ReadPrefixedString32();

      // TODO(adm244): are these ever used? investigate...
      if (Version >= 48) // 3.4.1
        ReadEngineCapabilities(r);

      AGSAlignedStream ar = new AGSAlignedStream(r);
      setup.LoadFromStream(ar, Version);

      if (Version > 32) // 3.x
        ReadSaveGameInfo(r);

      fonts = new AGSFont[setup.fonts_count];
      for (int i = 0; i < fonts.Length; ++i)
        fonts[i] = new AGSFont();

      if (Version >= 50) // 3.5.0
      {
        for (int i = 0; i < fonts.Length; ++i)
          fonts[i].ReadFromStream(r);
      }
      else
      {
        ReadOldFonts(r);
      }

      // parse sprite flags
      int sprites_count_max = 6000;
      if (Version >= 24) // pre 2.5.6
        sprites_count_max = r.ReadInt32();

      sprite_flags = r.ReadBytes(sprites_count_max);

      // parse inventory items info
      inventoryItems = new AGSInventoryItem[setup.inventory_items_count];
      AGSAlignedStream ar1 = new AGSAlignedStream(r);
      for (int i = 0; i < setup.inventory_items_count; ++i)
      {
        inventoryItems[i] = new AGSInventoryItem();
        inventoryItems[i].LoadFromStream(ar1);
        //NOTE(adm244): reset aligned stream??
      }

      // parse cursors info
      AGSAlignedStream ar2 = new AGSAlignedStream(r);
      cursors = new AGSCursorInfo[setup.cursors_count];
      for (int i = 0; i < cursors.Length; ++i)
      {
        cursors[i] = new AGSCursorInfo();
        cursors[i].LoadFromStream(ar2);
      }

      characters = new AGSCharacter[setup.characters_count];

      if (Version > 32) // 3.x
      {
        // parse characters interaction scripts
        for (int i = 0; i < characters.Length; ++i)
        {
          characters[i] = new AGSCharacter();
          characters[i].interactions.LoadFromStream(r);
        }

        // parse inventory items interaction scripts
        // FIXME(adm244): shouldn't it be zero-based?
        for (int i = 1; i < inventoryItems.Length; ++i)
        {
          //inventoryItems[i] = new AGSInventoryItem();
          inventoryItems[i].interactions.LoadFromStream(r);
        }
      }
      else // 2.72 and older
      {
        for (int i = 0; i < characters.Length; ++i)
        {
          characters[i] = new AGSCharacter();
          characters[i].interactions_old.LoadFromStream(r);
        }

        for (int i = 0; i < inventoryItems.Length; ++i)
        {
          //inventoryItems[i] = new AGSInventoryItem();
          inventoryItems[i].interactions_old.LoadFromStream(r);
        }

        Int32 globalvars_count = r.ReadInt32();
        globalvars = new AGSInteractionVariable[globalvars_count];
        for (int i = 0; i < globalvars.Length; ++i)
        {
          globalvars[i] = new AGSInteractionVariable();
          globalvars[i].LoadFromStream(r);
        }
      }

      // parse dictionary
      if (setup.load_dictionary != 0)
        dictionary.LoadFromStream(r);

      // parse global script
      globalScript.ReadFromStream(r);

      // parse dialog script
      if (Version > 37) // 3.1.0
        dialogScript.ReadFromStream(r);

      // parse other scripts
      if (Version >= 31) // 2.7.0
      {
        Int32 modules_count = r.ReadInt32();
        scriptModules = new AGSScript[modules_count];
        for (int i = 0; i < scriptModules.Length; ++i)
        {
          scriptModules[i] = new AGSScript();
          scriptModules[i].ReadFromStream(r);
        }
      }

      // parse views
      if (Version > 32) // 3.x
      {
        views = new AGSView[setup.views_count];
        for (int i = 0; i < views.Length; ++i)
        {
          views[i] = new AGSView();
          views[i].LoadFromStream(r);
        }
      }
      else // 2.7.2 and older
      {
        views272 = new AGSView272[setup.views_count];
        for (int i = 0; i < views272.Length; ++i)
        {
          views272[i] = new AGSView272();
          views272[i].LoadFromStream(r);
        }
      }

      // TODO(adm244): investigate "<= 2.1 skip unknown data"
      // !!! DON'T TRUST THE VERSION NUMBER HERE !!!
      //if (Version <= 19) // 2.1
      //{
      //  int count = r.ReadInt32();
      //  r.BaseStream.Seek(count * 0x204, SeekOrigin.Current);
      //}

      // parse characters
      AGSAlignedStream ar3 = new AGSAlignedStream(r);
      for (int i = 0; i < characters.Length; ++i)
      {
        characters[i].LoadFromStream(ar3);

        // FIXME(adm244): it's supposed to be 'ar3', but since it works just fine
        // maybe this shouldn't be reset after all?
        //ar.Reset();
      }

      // NOTE(adm244): according to "legacy source" lipsync data appears in "2.1"
      // which contradicts information gathered from reversing ags 2.54 engine;
      // trusting RE data for now, but check the actual "2.1" (or whatever this is) version
      //if (Version > 19) // 2.1
      if (Version >= 21)
      {
        LipSyncFrameLetters = new string[20];
        for (int i = 0; i < LipSyncFrameLetters.Length; ++i)
          LipSyncFrameLetters[i] = r.ReadFixedCString(50);
      }

      ParseGlobalMessages(r, Version);

      ParseDialogs(r);

      if (Version <= 37) // 3.0 and older
      {
        for (int i = 0; i < dialogs.Length; ++i)
        {
          dialogs[i].old_dialog_code = r.ReadBytes(dialogs[i].code_size);

          Int32 scriptSize = r.ReadInt32();
          byte[] encodedStr = r.ReadBytes(scriptSize);
          dialogs[i].old_dialog_script = AGSEncryption.DecryptAvis(encodedStr);
        }

        if (Version <= 25) // 2.60 and older
        {
          while (true)
          {
            uint mark = r.ReadUInt32();

            // NOTE(adm244): mark overlaps with a string
            r.BaseStream.Seek(-sizeof(UInt32), SeekOrigin.Current);

            if (mark == GUI_SIGNATURE)
              break;

            string dialogString = r.ReadCString();
            oldDialogStrings.Add(dialogString);
          }
        }
        else
        {
          while (true)
          {
            Int32 length = r.ReadInt32();
            if ((UInt32)length == GUI_SIGNATURE)
            {
              r.BaseStream.Seek(-sizeof(UInt32), SeekOrigin.Current);
              break;
            }

            byte[] encodedStr = r.ReadBytes(length);
            oldDialogStrings.Add(AGSEncryption.DecryptAvis(encodedStr));
          }
        }
      }

      int gui_version = ParseGUIs(r);
      ParseGUIControls(r, gui_version);

      // NOTE(adm244): "legacy source" (and all up to 3.6) got this wrong, it's 19+, NOT 25+
      // the only reason why it doesn't crash -- it's at the EOF
      if (Version > 18) // ???
        ParsePlugins(r);

      if (Version >= 25) // 2.60+
      {
        ParseCustomProperties(r);
        ParseObjectsScriptNames(r, Version);
      }

      if (Version >= 41) // 3.2.0+
      {
        audioStorage = new AGSAudioStorage();
        audioStorage.LoadFromStream(r);
      }

      if (Version >= 36) // 2.8 ???
        ParseRoomsDebugInfo(r);

      if (Version > 50) // > 3.5.0
      {
        //NOTE(adm244): don't try to read extension blocks if at the eof
        if (r.BaseStream.Position < r.BaseStream.Length)
        {
          ExtensionBlock.ReadMultiple(r, ReadExtensionBlock,
            ExtensionBlock.Options.Id8 | ExtensionBlock.Options.Size64);
        }
      }
    }

    private void WriteToStream(BinaryWriter writer)
    {
      writer.WriteFixedString(GameDataSignature, GameDataSignature.Length);

      writer.Write((Int32)Version);
      writer.WritePrefixedString32(VersionEngine);

      if (Version >= 48) // 3.4.1
        WriteEngineCapabilities(writer);

      AGSAlignedStream aw = new AGSAlignedStream(writer);
      setup.WriteToStream(aw, Version);

      if (Version > 32) // 3.x
        WriteSaveGameInfo(writer);

      if (Version >= 50) // 3.5.0
      {
        for (int i = 0; i < fonts.Length; ++i)
          fonts[i].WriteToStream(writer);
      }
      else
      {
        WriteOldFonts(writer);
      }

      if (Version >= 24) // ???
        writer.Write((Int32)sprite_flags.Length);

      writer.Write((byte[])sprite_flags);

      aw = new AGSAlignedStream(writer);
      for (int i = 0; i < inventoryItems.Length; ++i)
        inventoryItems[i].WriteToStream(aw);

      aw = new AGSAlignedStream(writer);
      for (int i = 0; i < cursors.Length; ++i)
        cursors[i].WriteToStream(aw);

      if (Version > 32) // 3.x
      {
        for (int i = 0; i < characters.Length; ++i)
          characters[i].interactions.WriteToStream(writer);

        // FIXME(adm244): shouldn't it be zero-based?
        for (int i = 1; i < inventoryItems.Length; ++i)
          inventoryItems[i].interactions.WriteToStream(writer);
      }
      else // 2.72 and older
      {
        for (int i = 0; i < characters.Length; ++i)
          characters[i].interactions_old.WriteToStream(writer);

        for (int i = 0; i < inventoryItems.Length; ++i)
          inventoryItems[i].interactions_old.WriteToStream(writer);

        writer.Write((Int32)globalvars.Length);
        for (int i = 0; i < globalvars.Length; ++i)
          globalvars[i].WriteToStream(writer);
      }

      if (setup.load_dictionary != 0)
        dictionary.WriteToStream(writer);

      globalScript.WriteToStream(writer);

      if (Version > 37) // 3.1.0
        dialogScript.WriteToStream(writer);

      if (Version >= 31) // 2.70
      {
        writer.Write((Int32)scriptModules.Length);
        for (int i = 0; i < scriptModules.Length; ++i)
          scriptModules[i].WriteToStream(writer);
      }

      if (Version > 32) // 3.x
      {
        for (int i = 0; i < views.Length; ++i)
          views[i].WriteToStream(writer);
      }
      else
      {
        for (int i = 0; i < views272.Length; ++i)
          views272[i].WriteToStream(writer);
      }

      aw = new AGSAlignedStream(writer);
      for (int i = 0; i < characters.Length; ++i)
        characters[i].WriteToStream(aw);

      // NOTE(adm244): see read function for info
      //if (Version >= 21) // 2.54
      if (Version > 19) // ???
      {
        Debug.Assert(LipSyncFrameLetters.Length == 20);
        for (int i = 0; i < LipSyncFrameLetters.Length; ++i)
          writer.WriteFixedString(LipSyncFrameLetters[i], 50);
      }

      WriteGlobalMessages(writer, Version);

      WriteDialogs(writer);

      if (Version <= 37) // 3.0 and older
      {
        for (int i = 0; i < dialogs.Length; ++i)
        {
          writer.Write((byte[])dialogs[i].old_dialog_code);

          byte[] encodedStr = AGSEncryption.EncryptAvis(dialogs[i].old_dialog_script);
          writer.Write((Int32)encodedStr.Length);
          writer.Write((byte[])encodedStr);
        }


        for (int i = 0; i < oldDialogStrings.Count; ++i)
        {
          if (Version <= 25) // 2.60 and older
          {
            writer.WriteCString(oldDialogStrings[i]);
          }
          else
          {
            byte[] encodedStr = AGSEncryption.EncryptAvis(oldDialogStrings[i]);
            writer.Write((Int32)encodedStr.Length);
            writer.Write((byte[])encodedStr);
          }
        }
      }

      WriteGUIs(writer);
      WriteGUIsControls(writer, GUIVersion);

      if (Version >= 18) // ???
        WritePlugins(writer);

      if (Version >= 25) // 2.60+
      {
        WriteCustomProperties(writer);
        WriteObjectsScriptNames(writer, Version);
      }

      if (Version >= 41) // 3.2.0+
        audioStorage.WriteToStream(writer);

      if (Version >= 36) // 2.8 ???
        WriteRoomDebugInfo(writer);

      if (Version > 50) // > 3.5.0
      {
        ExtensionBlock.Options options = ExtensionBlock.Options.Id8 | ExtensionBlock.Options.Size64;

        for (int i = 0; i < ExtensionBlocks.Count; ++i)
          ExtensionBlock.WriteSingle(writer, ExtensionBlocks[i], WriteExtensionBlock, options);

        if (ExtensionBlocks.Count > 0)
          ExtensionBlock.WriteEndOfFile(writer, options);
      }
    }

    private bool ReadEngineCapabilities(BinaryReader reader)
    {
      int count = reader.ReadInt32();

      Capabilities = new string[count];
      for (int i = 0; i < Capabilities.Length; ++i)
        Capabilities[i] = reader.ReadPrefixedString32();

      return true;
    }

    private void WriteEngineCapabilities(BinaryWriter writer)
    {
      writer.Write((Int32)Capabilities.Length);

      for (int i = 0; i < Capabilities.Length; ++i)
        writer.WritePrefixedString32(Capabilities[i]);
    }

    private bool ReadSaveGameInfo(BinaryReader reader)
    {
      // FIXME(adm244): read as string
      save_guid = reader.ReadChars(40);
      save_extension = reader.ReadChars(20);
      save_folder = reader.ReadChars(50);

      return true;
    }

    private void WriteSaveGameInfo(BinaryWriter writer)
    {
      writer.Write((char[])save_guid);
      writer.Write((char[])save_extension);
      writer.Write((char[])save_folder);
    }

    private bool ReadOldFonts(BinaryReader reader)
    {
      for (int i = 0; i < fonts.Length; ++i)
        fonts[i].Flags = reader.ReadByte();

      for (int i = 0; i < fonts.Length; ++i)
        fonts[i].Outline = reader.ReadByte();

      if (Version >= 48) // 3.4.1
      {
        for (int i = 0; i < fonts.Length; ++i)
        {
          fonts[i].OffsetY = reader.ReadInt32();

          if (Version >= 49)
            fonts[i].LineSpacing = reader.ReadInt32();
        }
      }

      return true;
    }

    private void WriteOldFonts(BinaryWriter writer)
    {
      for (int i = 0; i < fonts.Length; ++i)
        writer.Write((byte)fonts[i].Flags);

      for (int i = 0; i < fonts.Length; ++i)
        writer.Write((byte)fonts[i].Outline);

      if (Version >= 48) // 3.4.1
      {
        for (int i = 0; i < fonts.Length; ++i)
        {
          writer.Write((Int32)fonts[i].OffsetY);

          if (Version >= 49)
            writer.Write((Int32)fonts[i].LineSpacing);
        }
      }
    }

    private bool ReadFontsExtensionBlock(BinaryReader reader)
    {
      font_outlines_thickness = new int[setup.fonts_count];
      font_outlines_style = new int[setup.fonts_count];

      for (int i = 0; i < setup.fonts_count; ++i)
      {
        font_outlines_thickness[i] = reader.ReadInt32();
        font_outlines_style[i] = reader.ReadInt32();

        //NOTE(adm244): skip over reserved ints
        // (btw, why do they reserve space if there's block size right there
        //  and it's trivial to skip over unknown options and add extra stuff?)
        reader.BaseStream.Seek(4 * sizeof(Int32), SeekOrigin.Current);
      }

      return true;
    }

    private bool WriteFontsExtensionBlock(BinaryWriter writer)
    {
      for (int i = 0; i < setup.fonts_count; ++i)
      {
        writer.Write((Int32)font_outlines_thickness[i]);
        writer.Write((Int32)font_outlines_style[i]);

        writer.WriteArrayInt32(new Int32[4]); // reserved
      }

      return true;
    }

    private bool ReadCursorsExtensionBlock(BinaryReader reader)
    {
      for (int i = 0; i < setup.cursors_count; ++i)
      {
        cursors[i].animdelay = reader.ReadInt32();

        //NOTE(adm244): skip over reserved ints
        reader.BaseStream.Seek(3 * sizeof(Int32), SeekOrigin.Current);
      }

      return true;
    }

    private bool WriteCursorsExtensionBlock(BinaryWriter writer)
    {
      for (int i = 0; i < setup.cursors_count; ++i)
      {
        writer.Write((Int32)cursors[i].animdelay);

        writer.WriteArrayInt32(new Int32[3]); // reserved
      }

      return true;
    }

    private bool ReadExtensionBlock(BinaryReader reader, string id, long size)
    {
      bool result = false;

      switch (id)
      {
        case "v360_fonts":
          result = ReadFontsExtensionBlock(reader);
          break;

        case "v360_cursors":
          result = ReadCursorsExtensionBlock(reader);
          break;

        default:
          Debug.Assert(false, $"Data extension block '{id}' is not supported!");
          break;
      }

      if (result)
        ExtensionBlocks.Add(id);

      return result;
    }

    private bool WriteExtensionBlock(BinaryWriter writer, string id)
    {
      switch (id)
      {
        case "v360_fonts":
          return WriteFontsExtensionBlock(writer);

        case "v360_cursors":
          return WriteCursorsExtensionBlock(writer);

        default:
          throw new NotSupportedException($"Unknown game data extension block: {id}.");
      }
    }

    private void ParseRoomsDebugInfo(BinaryReader r)
    {
      if (setup.options[0] == 0)
        return;

      Int32 count = r.ReadInt32();
      roomsDebugInfo = new AGSRoomDebugInfo[count];
      for (int i = 0; i < roomsDebugInfo.Length; ++i)
      {
        roomsDebugInfo[i].id = r.ReadInt32();
        roomsDebugInfo[i].name = r.ReadCString(3000);
      }
    }

    private void WriteRoomDebugInfo(BinaryWriter writer)
    {
      if (setup.options[0] == 0)
        return;

      writer.Write((Int32)roomsDebugInfo.Length);
      for (int i = 0; i < roomsDebugInfo.Length; ++i)
      {
        writer.Write((Int32)roomsDebugInfo[i].id);
        writer.WriteCString(roomsDebugInfo[i].name, 3000);
      }
    }

    private void ParseObjectsScriptNames(BinaryReader r, int dta_version)
    {
      // parse views script names
      for (int i = 0; i < setup.views_count; ++i)
      {
        if (dta_version > 32) // 3.x
          views[i].scriptName = r.ReadCString();
        else
          views272[i].scriptName = r.ReadCString();
      }

      // parse inventory items script names
      for (int i = 0; i < setup.inventory_items_count; ++i)
        inventoryItems[i].scriptName = r.ReadCString();

      // parse dialogs script names
      for (int i = 0; i < setup.dialogs_count; ++i)
        dialogs[i].scriptName = r.ReadCString();
    }

    private void WriteObjectsScriptNames(BinaryWriter writer, int dta_version)
    {
      for (int i = 0; i < setup.views_count; ++i)
      {
        if (dta_version > 32) // 3.x
          writer.WriteCString(views[i].scriptName);
        else
          writer.WriteCString(views272[i].scriptName);
      }

      for (int i = 0; i < setup.inventory_items_count; ++i)
        writer.WriteCString(inventoryItems[i].scriptName);

      for (int i = 0; i < setup.dialogs_count; ++i)
        writer.WriteCString(dialogs[i].scriptName);
    }

    private void ParseCustomProperties(BinaryReader r)
    {
      //TODO(adm244): investigate if it should be an array
      customPropertiesSchema = new AGSCustomPropertiesSchema();
      customPropertiesSchema.LoadFromStream(r);

      // parse characters properties
      for (int i = 0; i < setup.characters_count; ++i)
      {
        characters[i].properties = new AGSPropertyStorage();
        characters[i].properties.ReadFromStream(r);
      }

      // parse inventory items properties
      for (int i = 0; i < setup.inventory_items_count; ++i)
      {
        inventoryItems[i].properties = new AGSPropertyStorage();
        inventoryItems[i].properties.ReadFromStream(r);
      }
    }

    private void WriteCustomProperties(BinaryWriter writer)
    {
      customPropertiesSchema.WriteToStream(writer);

      for (int i = 0; i < setup.characters_count; ++i)
        characters[i].properties.WriteToStream(writer);

      for (int i = 0; i < setup.inventory_items_count; ++i)
        inventoryItems[i].properties.WriteToStream(writer);
    }

    private void ParsePlugins(BinaryReader r)
    {
      PluginVersion = r.ReadInt32();
      if (PluginVersion != 1)
        throw new NotSupportedException($"Unsupported plugins format version: {PluginVersion}.");

      Int32 count = r.ReadInt32();
      Plugins = new AGSPluginInfo[count];
      for (int i = 0; i < Plugins.Length; ++i)
      {
        Plugins[i] = new AGSPluginInfo();
        Plugins[i].ReadFromStream(r, PluginVersion);
      }
    }

    private void WritePlugins(BinaryWriter writer)
    {
      writer.Write((Int32)PluginVersion);

      // TODO(adm244): check engine limits before writing
      // 2.54:
      //  plugins count: 20
      //  plugin data size: 0x1400

      writer.Write((Int32)Plugins.Length);
      for (int i = 0; i < Plugins.Length; ++i)
        Plugins[i].WriteToStream(writer, PluginVersion);
    }

    private void ParseGUIControls(BinaryReader r, int gui_version)
    {
      // parse controls
      Int32 buttons_count = r.ReadInt32();
      buttons = new AGSGUIButton[buttons_count];
      for (int i = 0; i < buttons.Length; ++i)
      {
        buttons[i] = new AGSGUIButton();
        buttons[i].LoadFromStream(r, gui_version);
      }

      Int32 labels_count = r.ReadInt32();
      labels = new AGSGUILabel[labels_count];
      for (int i = 0; i < labels.Length; ++i)
      {
        //TODO(adm244): test that on a real dta file
        labels[i] = new AGSGUILabel();
        labels[i].LoadFromStream(r, gui_version);
      }

      Int32 invwindows_count = r.ReadInt32();
      inventoryWindows = new AGSGUIInventoryWindow[invwindows_count];
      for (int i = 0; i < inventoryWindows.Length; ++i)
      {
        inventoryWindows[i] = new AGSGUIInventoryWindow();
        inventoryWindows[i].LoadFromStream(r, gui_version);
      }

      if (gui_version >= 100)
      {
        Int32 sliders_count = r.ReadInt32();
        sliders = new AGSGUISlider[sliders_count];
        for (int i = 0; i < sliders.Length; ++i)
        {
          //TODO(adm244): test that on a real dta file
          sliders[i] = new AGSGUISlider();
          sliders[i].LoadFromStream(r, gui_version);
        }
      }

      if (gui_version >= 101)
      {
        Int32 textboxes_count = r.ReadInt32();
        textboxes = new AGSGUITextBox[textboxes_count];
        for (int i = 0; i < textboxes.Length; ++i)
        {
          //TODO(adm244): test that on a real dta file
          textboxes[i] = new AGSGUITextBox();
          textboxes[i].LoadFromStream(r, gui_version);
        }
      }

      if (gui_version >= 102)
      {
        Int32 listboxes_count = r.ReadInt32();
        listboxes = new AGSGUIListBox[listboxes_count];
        for (int i = 0; i < listboxes.Length; ++i)
        {
          //TODO(adm244): test that on a real dta file
          listboxes[i] = new AGSGUIListBox();
          listboxes[i].LoadFromStream(r, gui_version);
        }
      }
    }

    private void WriteGUIsControls(BinaryWriter writer, int version)
    {
      writer.Write((Int32)buttons.Length);
      for (int i = 0; i < buttons.Length; ++i)
        buttons[i].WriteToStream(writer, version);

      writer.Write((Int32)labels.Length);
      for (int i = 0; i < labels.Length; ++i)
        labels[i].WriteToStream(writer, version);

      writer.Write((Int32)inventoryWindows.Length);
      for (int i = 0; i < inventoryWindows.Length; ++i)
        inventoryWindows[i].WriteToStream(writer, version);

      if (version >= 100) // ???
      {
        writer.Write((Int32)sliders.Length);
        for (int i = 0; i < sliders.Length; ++i)
          sliders[i].WriteToStream(writer, version);
      }

      if (version >= 101) // ???
      {
        writer.Write((Int32)textboxes.Length);
        for (int i = 0; i < textboxes.Length; ++i)
          textboxes[i].WriteToStream(writer, version);
      }

      if (version >= 102) // ???
      {
        writer.Write((Int32)listboxes.Length);
        for (int i = 0; i < listboxes.Length; ++i)
          listboxes[i].WriteToStream(writer, version);
      }
    }

    private int ParseGUIs(BinaryReader r)
    {
      // verify signature
      Int32 signature = r.ReadInt32();
      Debug.Assert((UInt32)signature == GUI_SIGNATURE);

      // parse header
      GUIVersion = r.ReadInt32();
      Int32 count = r.ReadInt32();
      Debug.Assert((count >= 0) && (count <= 1000));

      // parse guis
      guis = new AGSGUI[count];
      for (int i = 0; i < guis.Length; ++i)
      {
        guis[i] = new AGSGUI();
        guis[i].LoadFromStream(r, GUIVersion);
      }

      return GUIVersion;
    }

    private void WriteGUIs(BinaryWriter writer)
    {
      writer.Write((UInt32)GUI_SIGNATURE);

      writer.Write((Int32)GUIVersion);
      writer.Write((Int32)guis.Length);

      for (int i = 0; i < guis.Length; ++i)
        guis[i].WriteToStream(writer, GUIVersion);
    }

    private void ParseDialogs(BinaryReader r)
    {
      dialogs = new AGSDialog[setup.dialogs_count];
      for (int i = 0; i < setup.dialogs_count; ++i)
      {
        dialogs[i] = new AGSDialog();
        dialogs[i].LoadFromStream(r);
      }
    }

    private void WriteDialogs(BinaryWriter writer)
    {
      for (int i = 0; i < setup.dialogs_count; ++i)
        dialogs[i].WriteToStream(writer);
    }

    private void ParseGlobalMessages(BinaryReader r, int dta_version)
    {
      globalMessages = new string[LIMIT_MAX_GLOBAL_MESSAGES];
      for (int i = 0; i < LIMIT_MAX_GLOBAL_MESSAGES; ++i)
      {
        if (setup.global_messages[i] == 0)
          continue;

        if (dta_version < 26) // 2.61
          globalMessages[i] = r.ReadCString();
        else
          globalMessages[i] = r.ReadEncryptedCString();
      }
    }

    private void WriteGlobalMessages(BinaryWriter writer, int dta_version)
    {
      for (int i = 0; i < globalMessages.Length; ++i)
      {
        if (setup.global_messages[i] == 0)
          continue;

        if (dta_version < 26) // 2.61
          writer.WriteCString(globalMessages[i]);
        else
          writer.WriteEncryptedCString(globalMessages[i]);
      }
    }
  }
}
