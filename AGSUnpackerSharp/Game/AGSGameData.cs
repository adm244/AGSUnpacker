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
      BinaryReader r = new BinaryReader(fs, Encoding.ASCII);

      // verify signature
      char[] dta_sig = r.ReadChars(DTA_SIGNATURE.Length);
      string dta_sig_string = new string(dta_sig);
      Debug.Assert(DTA_SIGNATURE == dta_sig_string);

      // parse dta header
      Int32 dta_version = r.ReadInt32();
      Debug.Assert(dta_version == 43);

      //TODO(adm244): parse it with ReadString?
      Int32 engine_version_strlen = r.ReadInt32();
      Debug.Assert(engine_version_strlen == 7);

      char[] engine_version = r.ReadChars(engine_version_strlen);
      string engine_version_string = new string(engine_version);
      Debug.Assert(engine_version_string == "3.3.4.2");

      // parse game setup struct base
      AGSAlignedStream ar = new AGSAlignedStream(r);
      setup.LoadFromStream(ar);

      // parse extended game setup struct (dtaver > 32)
      // parse save game info
      save_guid = r.ReadChars(40);
      Debug.Assert(r.BaseStream.Position == 0xF85);

      save_extension = r.ReadChars(20);
      Debug.Assert(r.BaseStream.Position == 0xF99);

      save_folder = r.ReadChars(50);
      Debug.Assert(r.BaseStream.Position == 0xFCB);

      // parse font info
      font_flags = r.ReadBytes(setup.fonts_count);
      Debug.Assert(r.BaseStream.Position == 0xFCE);

      font_outlines = r.ReadBytes(setup.fonts_count);
      Debug.Assert(r.BaseStream.Position == 0xFD1);

      // parse sprite flags
      // dtaver >= 24
      Int32 sprites_count_max = r.ReadInt32();
      Debug.Assert(r.BaseStream.Position == 0xFD5);

      sprite_flags = r.ReadBytes(sprites_count_max);
      Debug.Assert(r.BaseStream.Position == 0x8505);

      // parse inventory items info
      inventoryItems = new AGSInventoryItem[setup.inventory_items_count];
      AGSAlignedStream ar1 = new AGSAlignedStream(r);
      for (int i = 0; i < setup.inventory_items_count; ++i)
      {
        inventoryItems[i] = new AGSInventoryItem();
        inventoryItems[i].LoadFromStream(ar1);
        //NOTE(adm244): reset aligned stream??
      }
      Debug.Assert(r.BaseStream.Position == 0x869D);

      // parse cursors info
      AGSAlignedStream ar2 = new AGSAlignedStream(r);
      cursors = new AGSCursorInfo[setup.cursors_count];
      for (int i = 0; i < cursors.Length; ++i)
      {
        cursors[i] = new AGSCursorInfo();
        cursors[i].LoadFromStream(ar2);
      }
      Debug.Assert(r.BaseStream.Position == 0x878D);

      // parse characters interaction scripts
      characters = new AGSCharacter[setup.characters_count];
      for (int i = 0; i < characters.Length; ++i)
      {
        characters[i] = new AGSCharacter();
        characters[i].interactions.LoadFromStream(r);
      }
      Debug.Assert(r.BaseStream.Position == 0x8870);

      // parse inventory items interaction scripts
      for (int i = 1; i < setup.inventory_items_count; ++i)
      {
        inventoryItems[i] = new AGSInventoryItem();
        inventoryItems[i].interactions.LoadFromStream(r);
      }
      Debug.Assert(r.BaseStream.Position == 0x88FA);

      // parse dictionary
      if (setup.load_dictionary != 0)
      {
        dictionary.LoadFromStream(r);
        Debug.Assert(r.BaseStream.Position == 0x8A49);
      }

      // parse global script
      globalScript.LoadFromStream(r);
      Debug.Assert(r.BaseStream.Position == 0x11515);

      // parse dialog script
      if (dta_version > 37) // 3.1.0
      {
        dialogScript.LoadFromStream(r);
        Debug.Assert(r.BaseStream.Position == 0x11F4E);
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
        Debug.Assert(r.BaseStream.Position == 0x126D6);
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
        Debug.Assert(r.BaseStream.Position == 0x14250);
      }

      // parse characters
      AGSAlignedStream ar3 = new AGSAlignedStream(r);
      for (int i = 0; i < characters.Length; ++i)
      {
        characters[i].LoadFromStream(ar3);
        ar.Reset();
      }
      Debug.Assert(r.BaseStream.Position == 0x17310);

      // parse lipsync
      if (dta_version >= 21) // 2.54
      {
        //TODO(adm244): real parsing
        r.BaseStream.Seek(20 * 50, SeekOrigin.Current);
        Debug.Assert(r.BaseStream.Position == 0x176F8);
      }

      // parse global messages
      ParseGlobalMessages(r);
      Debug.Assert(r.BaseStream.Position == 0x176F8);

      // parse dialogs
      ParseDialogs(r);
      Debug.Assert(r.BaseStream.Position == 0x176F8);

      // parse guis
      ParseGUIs(r);

      // parse gui controls
      ParseGUIControls(r);
      Debug.Assert(r.BaseStream.Position == 0x1A611);

      // parse plugins
      ParsePlugins(r);
      Debug.Assert(r.BaseStream.Position == 0x1A619);

      // parse custom properties
      ParseCustomProperties(r);
      ParseObjectsScriptNames(r);
      Debug.Assert(r.BaseStream.Position == 0x1A7AA);

      // parse audio clips
      audioStorage = new AGSAudioStorage();
      audioStorage.LoadFromStream(r);
      Debug.Assert(r.BaseStream.Position == 0x1AD06);

      // parse rooms debug info
      ParseRoomsDebugInfo(r);
      Debug.Assert(r.BaseStream.Position == 0x1AE08);

      r.Close();
    }

    private void ParseRoomsDebugInfo(BinaryReader r)
    {
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
      Debug.Assert(r.BaseStream.Position == 0x1A77E);

      // parse inventory items script names
      for (int i = 0; i < setup.inventory_items_count; ++i)
      {
        inventoryItems[i].scriptName = r.ReadNullTerminatedString();
      }
      Debug.Assert(r.BaseStream.Position == 0x1A7AA);

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
      Debug.Assert(r.BaseStream.Position == 0x1A6D1);
    }

    private void ParsePlugins(BinaryReader r)
    {
      Int32 format = r.ReadInt32();
      Debug.Assert(format == 1);

      Int32 count = r.ReadInt32();
      for (int i = 0; i < count; ++i)
      {
        //TODO(adm244): parse plugins
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
      Debug.Assert(r.BaseStream.Position == 0x1A5BC);

      Int32 labels_count = r.ReadInt32();
      labels = new AGSGUILabel[labels_count];
      for (int i = 0; i < labels.Length; ++i)
      {
        //TODO(adm244): test that on a real dta file
        labels[i] = new AGSGUILabel();
        labels[i].LoadFromStream(r);
      }
      Debug.Assert(r.BaseStream.Position == 0x1A5C0);

      Int32 invwindows_count = r.ReadInt32();
      inventoryWindows = new AGSGUIInventoryWindow[invwindows_count];
      for (int i = 0; i < inventoryWindows.Length; ++i)
      {
        inventoryWindows[i] = new AGSGUIInventoryWindow();
        inventoryWindows[i].LoadFromStream(r);
      }
      Debug.Assert(r.BaseStream.Position == 0x1A605);

      Int32 sliders_count = r.ReadInt32();
      sliders = new AGSGUISlider[sliders_count];
      for (int i = 0; i < sliders.Length; ++i)
      {
        //TODO(adm244): test that on a real dta file
        sliders[i] = new AGSGUISlider();
        sliders[i].LoadFromStream(r);
      }
      Debug.Assert(r.BaseStream.Position == 0x1A609);

      Int32 textboxes_count = r.ReadInt32();
      textboxes = new AGSGUITextBox[textboxes_count];
      for (int i = 0; i < textboxes.Length; ++i)
      {
        //TODO(adm244): test that on a real dta file
        textboxes[i] = new AGSGUITextBox();
        textboxes[i].LoadFromStream(r);
      }
      Debug.Assert(r.BaseStream.Position == 0x1A60D);

      Int32 listboxes_count = r.ReadInt32();
      listboxes = new AGSGUIListBox[listboxes_count];
      for (int i = 0; i < listboxes.Length; ++i)
      {
        //TODO(adm244): test that on a real dta file
        listboxes[i] = new AGSGUIListBox();
        listboxes[i].LoadFromStream(r);
      }
      Debug.Assert(r.BaseStream.Position == 0x1A611);
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
        guis[i].LoadFromStream(r);
      }
    }

    private void ParseDialogs(BinaryReader r)
    {
      for (int i = 0; i < setup.dialogs_count; ++i)
      {
        //TODO(adm244): see DialogTopic::ReadFromFile
      }
    }

    private void ParseGlobalMessages(BinaryReader r)
    {
      for (int i = 0; i < LIMIT_MAX_GLOBAL_MESSAGES; ++i)
      {
        if (setup.global_messages[i] == 0) continue;
        // read encrypted string
        globalMessages[i] = AGSUtils.ReadEncryptedString(r);
      }
    }
  }
}
