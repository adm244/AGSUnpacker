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
    private static readonly string DTA_SIGNATURE = "Adventure Creator Game File v2";
    private static readonly UInt32 GUI_SIGNATURE = 0xCAFEBEEF;

    private static readonly int LIMIT_MAX_GLOBAL_MESSAGES = 500;

    public AGSGameSetupStruct setup;
    public char[] save_guid;
    public char[] save_extension;
    public char[] save_folder;
    public byte[] font_flags;
    public byte[] font_outlines;
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
    public string[] globalMessages;
    public AGSDialog[] dialogs;
    public AGSAudioStorage audioStorage;
    public AGSCustomProperiesSchema customPropertiesSchema;
    public AGSRoomDebugInfo[] roomsDebugInfo;
    public AGSInteractionVariable[] globalvars;

    public AGSView272[] views272;
    public List<string> oldDialogStrings;

    public AGSGUI[] guis;
    public AGSGUIButton[] buttons;
    public AGSGUILabel[] labels;
    public AGSGUIInventoryWindow[] inventoryWindows;
    public AGSGUISlider[] sliders;
    public AGSGUITextBox[] textboxes;
    public AGSGUIListBox[] listboxes;

    public AGSGameData()
    {
      setup = new AGSGameSetupStruct();
      save_guid = new char[0];
      save_extension = new char[0];
      save_folder = new char[0];
      font_flags = new byte[0];
      font_outlines = new byte[0];
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
      globalMessages = new string[0];
      dialogs = new AGSDialog[0];
      audioStorage = new AGSAudioStorage();
      customPropertiesSchema = new AGSCustomProperiesSchema();
      roomsDebugInfo = new AGSRoomDebugInfo[0];
      globalvars = new AGSInteractionVariable[0];

      views272 = new AGSView272[0];
      oldDialogStrings = new List<string>();

      guis = new AGSGUI[0];
      buttons = new AGSGUIButton[0];
      labels = new AGSGUILabel[0];
      inventoryWindows = new AGSGUIInventoryWindow[0];
      sliders = new AGSGUISlider[0];
      textboxes = new AGSGUITextBox[0];
      listboxes = new AGSGUIListBox[0];
    }

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

    public void LoadFromStream(BinaryReader r)
    {
      // verify signature
      char[] dta_sig = r.ReadChars(DTA_SIGNATURE.Length);
      string dta_sig_string = new string(dta_sig);
      Debug.Assert(DTA_SIGNATURE == dta_sig_string);

      // parse dta header
      Int32 dta_version = r.ReadInt32();
      Int32 strlen = r.ReadInt32();
      string engine_version = r.ReadFixedCString(strlen);

      // parse capability strings
      if (dta_version >= 48) // 3.4.1
      {
        Int32 capabilities_count = r.ReadInt32();
        for (int i = 0; i < capabilities_count; ++i)
        {
          string capability = r.ReadString();
        }
      }

      // parse game setup struct base
      AGSAlignedStream ar = new AGSAlignedStream(r);
      setup.LoadFromStream(ar, dta_version);

      if (dta_version > 32) // 3.x
      {
        // parse save game info
        save_guid = r.ReadChars(40);
        save_extension = r.ReadChars(20);
        save_folder = r.ReadChars(50);
      }

      // parse font info
      if (dta_version < 50) // 3.5
      {
        font_flags = r.ReadBytes(setup.fonts_count);
        font_outlines = r.ReadBytes(setup.fonts_count);
        if (dta_version >= 48) // 3.4.1
        {
          for (int i = 0; i < setup.fonts_count; ++i)
          {
            Int32 font_offset_y = r.ReadInt32();
            if (dta_version >= 49) // 3.4.1_2
            {
              Int32 font_linespacing = r.ReadInt32();
            }
          }
        }
      }
      else
      {
        for (int i = 0; i < setup.fonts_count; ++i)
        {
          Int32 flags = r.ReadInt32();
          Int32 sizePt = r.ReadInt32();
          Int32 outline = r.ReadInt32();
          Int32 yOffset = r.ReadInt32();
          Int32 lineSpacing = Math.Max(0, r.ReadInt32());
        }
      }

      // parse sprite flags
      Int32 sprites_count_max = 6000;
      if (dta_version >= 24) // pre 2.5.6
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

      if (dta_version > 32) // 3.x
      {
        // parse characters interaction scripts
        for (int i = 0; i < characters.Length; ++i)
        {
          characters[i] = new AGSCharacter();
          characters[i].interactions.LoadFromStream(r);
        }

        // parse inventory items interaction scripts
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
      if (dta_version > 37) // 3.1.0
        dialogScript.ReadFromStream(r);

      // parse other scripts
      if (dta_version >= 31) // 2.7.0
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
      if (dta_version > 32) // 3.x
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

      // parse characters
      AGSAlignedStream ar3 = new AGSAlignedStream(r);
      for (int i = 0; i < characters.Length; ++i)
      {
        characters[i].LoadFromStream(ar3);
        ar.Reset();
      }

      if (dta_version >= 21) // 2.54
      {
        //TODO(adm244): real parsing
        r.BaseStream.Seek(20 * 50, SeekOrigin.Current);
      }

      ParseGlobalMessages(r, dta_version);

      ParseDialogs(r);

      if (dta_version <= 37) // 3.0 and older
      {
        for (int i = 0; i < dialogs.Length; ++i)
        {
          dialogs[i].old_dialog_code = r.ReadBytes(dialogs[i].code_size);

          Int32 scriptSize = r.ReadInt32();
          byte[] encodedStr = r.ReadBytes(scriptSize);
          dialogs[i].old_dialog_script = AGSEncryption.DecryptAvis(encodedStr);
        }

        if (dta_version <= 25) // 2.60 and older
        {
          while (true)
          {
            uint mark = r.ReadUInt32();
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

      if (dta_version >= 25) // 2.60+
      {
        ParsePlugins(r);

        ParseCustomProperties(r);
        ParseObjectsScriptNames(r, dta_version);
      }

      if (dta_version >= 41) // 3.2.0+
      {
        audioStorage = new AGSAudioStorage();
        audioStorage.LoadFromStream(r);
      }

      if (dta_version >= 36) // 2.8 ???
      {
        ParseRoomsDebugInfo(r);
      }

      if (dta_version > 50) // > 3.5.0
      {
        //NOTE(adm244): don't try to read extension blocks if at the eof
        if (r.BaseStream.Position < r.BaseStream.Length)
        {
          ExtensionBlock.ReadMultiple(r, ReadExtensionBlock,
            ExtensionBlock.Options.Id8 | ExtensionBlock.Options.Size64);
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

    private bool ReadExtensionBlock(BinaryReader reader, string id, long size)
    {
      switch (id)
      {
        case "v360_fonts":
          return ReadFontsExtensionBlock(reader);

        case "v360_cursors":
          return ReadCursorsExtensionBlock(reader);

        default:
          Debug.Assert(false, $"Data extension block '{id}' is not supported!");
          return false;
      }
    }

    private void ParseRoomsDebugInfo(BinaryReader r)
    {
      if (setup.options[0] == 0) return;

      Int32 count = r.ReadInt32();
      roomsDebugInfo = new AGSRoomDebugInfo[count];
      for (int i = 0; i < roomsDebugInfo.Length; ++i)
      {
        roomsDebugInfo[i].id = r.ReadInt32();
        roomsDebugInfo[i].name = r.ReadCString(3000);
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
      {
        inventoryItems[i].scriptName = r.ReadCString();
      }

      // parse dialogs script names
      for (int i = 0; i < setup.dialogs_count; ++i)
      {
        dialogs[i].scriptName = r.ReadCString();
      }
    }

    private void ParseCustomProperties(BinaryReader r)
    {
      //TODO(adm244): investigate if it should be an array
      customPropertiesSchema = new AGSCustomProperiesSchema();
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

    private void ParsePlugins(BinaryReader r)
    {
      Int32 format = r.ReadInt32();
      Debug.Assert(format == 1);

      Int32 count = r.ReadInt32();
      for (int i = 0; i < count; ++i)
      {
        string name = r.ReadCString();
        Int32 datasize = r.ReadInt32();
        if (datasize > 0)
        {
          r.BaseStream.Seek(datasize, SeekOrigin.Current);
        }
      }
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

    private int ParseGUIs(BinaryReader r)
    {
      // verify signature
      Int32 signature = r.ReadInt32();
      Debug.Assert((UInt32)signature == GUI_SIGNATURE);

      // parse header
      Int32 version = r.ReadInt32();
      Int32 count = r.ReadInt32();
      Debug.Assert((count >= 0) && (count <= 1000));

      // parse guis
      guis = new AGSGUI[count];
      for (int i = 0; i < guis.Length; ++i)
      {
        guis[i] = new AGSGUI();
        guis[i].LoadFromStream(r, version);
      }

      return version;
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
  }
}
