using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace AGSUnpackerSharp
{
  public class AGSGameSetupStruct
  {
    public string name;
    public Int32[] options;
    public byte[] paluses;
    public Int32[] defaultPallete;
    public Int32 views_count;
    public Int32 characters_count;
    public Int32 player_character_id;
    public Int32 total_score;
    public Int16 inventory_items_count;
    public Int32 dialogs_count;
    public Int32 dialog_messages_count;
    public Int32 fonts_count;
    public Int32 color_depth;
    public Int32 target_win;
    public Int32 dialog_bullet;
    public Int16 hotdot;
    public Int16 hotdot_outter;
    public Int32 unique_id;
    public Int32 guis_count;
    public Int32 cursors_count;
    public Int32 default_resolution;
    public Int32 default_lipsync_frame;
    public Int32 inventory_hotdot_sprite;
    public Int32[] reserved;
    public Int32[] global_messages;
    public Int32 load_dictionary;
    public Int32 some_globalscript_value;
    public Int32 some_chars_value;
    public Int32 is_scriptcompiled;
    
    public AGSGameSetupStruct()
    {
      name = string.Empty;
      options = new Int32[0];
      paluses = new byte[0];
      defaultPallete = new Int32[0];
      views_count = 0;
      characters_count = 0;
      player_character_id = 0;
      total_score = 0;
      inventory_items_count = 0;
      dialogs_count = 0;
      dialog_messages_count = 0;
      fonts_count = 0;
      color_depth = 0;
      target_win = 0;
      dialog_bullet = 0;
      hotdot = 0;
      hotdot_outter = 0;
      unique_id = 0;
      guis_count = 0;
      cursors_count = 0;
      default_resolution = 0;
      default_lipsync_frame = 0;
      inventory_hotdot_sprite = 0;
      reserved = new Int32[0];
      global_messages = new Int32[0];
      load_dictionary = 0;
      some_globalscript_value = 0;
      some_chars_value = 0;
      is_scriptcompiled = 0;
    }

    public void LoadFromStream(AGSAlignedStream ar)
    {
      name = ar.ReadFixedString(50);
      Debug.Assert(ar.Position == 0x5F);

      options = ar.ReadArrayInt32(100);
      Debug.Assert(ar.Position == 0x1F1);

      paluses = ar.ReadBytes(256);
      Debug.Assert(ar.Position == 0x2F1);

      //NOTE(adm244): reg: ABGR; mem: RGBA
      defaultPallete = ar.ReadArrayInt32(256);
      Debug.Assert(ar.Position == 0x6F1);

      views_count = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x6F5);

      characters_count = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x6F9);

      player_character_id = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x6FD);

      total_score = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x701);

      inventory_items_count = ar.ReadInt16();
      Debug.Assert(ar.Position == 0x703);

      dialogs_count = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x709);

      dialog_messages_count = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x70D);

      fonts_count = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x711);

      color_depth = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x715);

      target_win = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x719);

      dialog_bullet = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x71D);

      hotdot = ar.ReadInt16();
      Debug.Assert(ar.Position == 0x71F);

      hotdot_outter = ar.ReadInt16();
      Debug.Assert(ar.Position == 0x721);

      unique_id = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x725);

      guis_count = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x729);

      cursors_count = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x72D);

      default_resolution = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x731);

      default_lipsync_frame = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x735);

      inventory_hotdot_sprite = ar.ReadInt32();
      Debug.Assert(ar.Position == 0x739);

      reserved = ar.ReadArrayInt32(17);
      Debug.Assert(ar.Position == 0x77D);

      global_messages = ar.ReadArrayInt32(500);
      Debug.Assert(ar.Position == 0xF4D);

      load_dictionary = ar.ReadInt32();
      Debug.Assert(ar.Position == 0xF51);

      some_globalscript_value = ar.ReadInt32();
      Debug.Assert(ar.Position == 0xF55);

      some_chars_value = ar.ReadInt32();
      Debug.Assert(ar.Position == 0xF59);

      is_scriptcompiled = ar.ReadInt32();
      Debug.Assert(ar.Position == 0xF5D);
    }
  }
}
