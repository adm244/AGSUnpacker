using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using AGSUnpackerSharp.Shared;

namespace AGSUnpackerSharp.Game
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
      FileStream fs = new FileStream(filepath, FileMode.Open);
      BinaryReader r = new BinaryReader(fs, Encoding.GetEncoding(1252));

      // verify signature
      char[] dta_sig = r.ReadChars(DTA_SIGNATURE.Length);
      string dta_sig_string = new string(dta_sig);
      Debug.Assert(DTA_SIGNATURE == dta_sig_string);

      // parse dta header
      Int32 dta_version = r.ReadInt32();
      Int32 strlen = r.ReadInt32();
      string engine_version = r.ReadFixedString(strlen);

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
      //Debug.Assert(r.BaseStream.Position == 0xF6A);

      // parse extended game setup struct (dtaver > 32)
      // parse save game info
      save_guid = r.ReadChars(40);
      save_extension = r.ReadChars(20);
      save_folder = r.ReadChars(50);
      //Debug.Assert(r.BaseStream.Position == 0xFD8);

      // parse font info
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
      //Debug.Assert(r.BaseStream.Position == 0x1096);

      // parse sprite flags
      // dtaver >= 24
      Int32 sprites_count_max = r.ReadInt32();
      sprite_flags = r.ReadBytes(sprites_count_max);
      //Debug.Assert(r.BaseStream.Position == 0x85CA);

      // parse inventory items info
      inventoryItems = new AGSInventoryItem[setup.inventory_items_count];
      AGSAlignedStream ar1 = new AGSAlignedStream(r);
      for (int i = 0; i < setup.inventory_items_count; ++i)
      {
        inventoryItems[i] = new AGSInventoryItem();
        inventoryItems[i].LoadFromStream(ar1);
        //NOTE(adm244): reset aligned stream??
      }
      //Debug.Assert(r.BaseStream.Position == 0x992E);

      // parse cursors info
      AGSAlignedStream ar2 = new AGSAlignedStream(r);
      cursors = new AGSCursorInfo[setup.cursors_count];
      for (int i = 0; i < cursors.Length; ++i)
      {
        cursors[i] = new AGSCursorInfo();
        cursors[i].LoadFromStream(ar2);
      }
      //Debug.Assert(r.BaseStream.Position == 0x9A36);

      // parse characters interaction scripts
      characters = new AGSCharacter[setup.characters_count];
      for (int i = 0; i < characters.Length; ++i)
      {
        characters[i] = new AGSCharacter();
        characters[i].interactions.LoadFromStream(r);
      }
      //Debug.Assert(r.BaseStream.Position == 0xA3D0);

      // parse inventory items interaction scripts
      for (int i = 1; i < setup.inventory_items_count; ++i)
      {
        inventoryItems[i] = new AGSInventoryItem();
        inventoryItems[i].interactions.LoadFromStream(r);
      }
      //Debug.Assert(r.BaseStream.Position == 0xA81C);

      // parse dictionary
      if (setup.load_dictionary != 0)
      {
        dictionary.LoadFromStream(r);
      }
      //Debug.Assert(r.BaseStream.Position == 0xA96B);

      // parse global script
      globalScript.LoadFromStream(r);
      //Debug.Assert(r.BaseStream.Position == 0xAFA44);

      // parse dialog script
      if (dta_version > 37) // 3.1.0
      {
        dialogScript.LoadFromStream(r);
        //Debug.Assert(r.BaseStream.Position == 0x404CD3);
      }

      // parse other scripts
      if (dta_version >= 31) // 2.7.0
      {
        Int32 modules_count = r.ReadInt32();
        scriptModules = new AGSScript[modules_count];
        for (int i = 0; i < scriptModules.Length; ++i)
        {
          scriptModules[i] = new AGSScript();
          scriptModules[i].LoadFromStream(r);
        }
        //Debug.Assert(r.BaseStream.Position == 0x639860);
      }

      // parse views
      if (dta_version > 32) // 2.7.2
      {
        views = new AGSView[setup.views_count];
        for (int i = 0; i < views.Length; ++i)
        {
          views[i] = new AGSView();
          views[i].LoadFromStream(r);
        }
        //Debug.Assert(r.BaseStream.Position == 0x6BEB3A);
      }

      // parse characters
      AGSAlignedStream ar3 = new AGSAlignedStream(r);
      for (int i = 0; i < characters.Length; ++i)
      {
        characters[i].LoadFromStream(ar3);
        ar.Reset();
      }
      //Debug.Assert(r.BaseStream.Position == 0x6D0CAE);

      // parse lipsync
      if (dta_version >= 21) // 2.54
      {
        //TODO(adm244): real parsing
        r.BaseStream.Seek(20 * 50, SeekOrigin.Current);
        //Debug.Assert(r.BaseStream.Position == 0x6D1096);
      }

      // parse global messages
      ParseGlobalMessages(r);
      //Debug.Assert(r.BaseStream.Position == 0x6D1096);

      // parse dialogs
      ParseDialogs(r);
      //Debug.Assert(r.BaseStream.Position == 0x7DB056);

      // parse guis
      ParseGUIs(r);

      // parse gui controls
      ParseGUIControls(r);
      //Debug.Assert(r.BaseStream.Position == 0x7E8738);

      // parse plugins
      ParsePlugins(r);
      //Debug.Assert(r.BaseStream.Position == 0x7E8758);

      // parse custom properties
      ParseCustomProperties(r);
      ParseObjectsScriptNames(r);
      //Debug.Assert(r.BaseStream.Position == 0x7EAE1D);

      // parse audio clips
      audioStorage = new AGSAudioStorage();
      audioStorage.LoadFromStream(r);
      //Debug.Assert(r.BaseStream.Position == 0x7FEBC9);

      // parse rooms debug info
      ParseRoomsDebugInfo(r);
      //Debug.Assert(r.BaseStream.Position == 0x7FEBC9);
      
      r.Close();
    }

    private void ParseRoomsDebugInfo(BinaryReader r)
    {
      if (setup.options[0] == 0) return;

      Int32 count = r.ReadInt32();
      roomsDebugInfo = new AGSRoomDebugInfo[count];
      for (int i = 0; i < roomsDebugInfo.Length; ++i)
      {
        roomsDebugInfo[i].id = r.ReadInt32();
        roomsDebugInfo[i].name = r.ReadNullTerminatedString(3000);
      }
    }

    private void ParseObjectsScriptNames(BinaryReader r)
    {
      // parse views script names
      for (int i = 0; i < setup.views_count; ++i)
      {
        views[i].scriptName = r.ReadNullTerminatedString();
      }

      // parse inventory items script names
      for (int i = 0; i < setup.inventory_items_count; ++i)
      {
        inventoryItems[i].scriptName = r.ReadNullTerminatedString();
      }

      // parse dialogs script names
      for (int i = 0; i < setup.dialogs_count; ++i)
      {
        dialogs[i].scriptName = r.ReadNullTerminatedString();
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
        characters[i].properties.LoadFromStream(r);
      }

      // parse inventory items properties
      for (int i = 0; i < setup.inventory_items_count; ++i)
      {
        inventoryItems[i].properties = new AGSPropertyStorage();
        inventoryItems[i].properties.LoadFromStream(r);
      }
      //Debug.Assert(r.BaseStream.Position == 0x7E912D);
    }

    private void ParsePlugins(BinaryReader r)
    {
      Int32 format = r.ReadInt32();
      Debug.Assert(format == 1);

      Int32 count = r.ReadInt32();
      for (int i = 0; i < count; ++i)
      {
        string name = r.ReadNullTerminatedString();
        Int32 datasize = r.ReadInt32();
        if (datasize > 0)
        {
          r.BaseStream.Seek(datasize, SeekOrigin.Current);
        }
      }
    }

    private void ParseGUIControls(BinaryReader r)
    {
      // parse controls
      Int32 buttons_count = r.ReadInt32();
      buttons = new AGSGUIButton[buttons_count];
      for (int i = 0; i < buttons.Length; ++i)
      {
        buttons[i] = new AGSGUIButton();
        buttons[i].LoadFromStream(r);
      }
      //Debug.Assert(r.BaseStream.Position == 0x7E63FD);

      Int32 labels_count = r.ReadInt32();
      labels = new AGSGUILabel[labels_count];
      for (int i = 0; i < labels.Length; ++i)
      {
        //TODO(adm244): test that on a real dta file
        labels[i] = new AGSGUILabel();
        labels[i].LoadFromStream(r);
      }
      //Debug.Assert(r.BaseStream.Position == 0x7E7D3C);

      Int32 invwindows_count = r.ReadInt32();
      inventoryWindows = new AGSGUIInventoryWindow[invwindows_count];
      for (int i = 0; i < inventoryWindows.Length; ++i)
      {
        inventoryWindows[i] = new AGSGUIInventoryWindow();
        inventoryWindows[i].LoadFromStream(r);
      }
      //Debug.Assert(r.BaseStream.Position == 0x7E7D81);

      Int32 sliders_count = r.ReadInt32();
      sliders = new AGSGUISlider[sliders_count];
      for (int i = 0; i < sliders.Length; ++i)
      {
        //TODO(adm244): test that on a real dta file
        sliders[i] = new AGSGUISlider();
        sliders[i].LoadFromStream(r);
      }
      //Debug.Assert(r.BaseStream.Position == 0x7E80BF);

      Int32 textboxes_count = r.ReadInt32();
      textboxes = new AGSGUITextBox[textboxes_count];
      for (int i = 0; i < textboxes.Length; ++i)
      {
        //TODO(adm244): test that on a real dta file
        textboxes[i] = new AGSGUITextBox();
        textboxes[i].LoadFromStream(r);
      }
      //Debug.Assert(r.BaseStream.Position == 0x7E8537);

      Int32 listboxes_count = r.ReadInt32();
      listboxes = new AGSGUIListBox[listboxes_count];
      for (int i = 0; i < listboxes.Length; ++i)
      {
        //TODO(adm244): test that on a real dta file
        listboxes[i] = new AGSGUIListBox();
        listboxes[i].LoadFromStream(r);
      }
    }

    private void ParseGUIs(BinaryReader r)
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

    private void ParseGlobalMessages(BinaryReader r)
    {
      globalMessages = new string[LIMIT_MAX_GLOBAL_MESSAGES];
      for (int i = 0; i < LIMIT_MAX_GLOBAL_MESSAGES; ++i)
      {
        if (setup.global_messages[i] == 0) continue;
        // read encrypted string
        globalMessages[i] = AGSStringUtils.ReadEncryptedString(r);
      }
    }
  }
}
