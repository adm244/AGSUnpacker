using System;

namespace AGSUnpacker.Lib
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

    public Int32 window_width;
    public Int32 window_height;
    
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

      window_width = 0;
      window_height = 0;
    }

    public void LoadFromStream(AGSAlignedStream ar, int dta_version)
    {
      name = ar.ReadFixedString(50);
      options = ar.ReadArrayInt32(100);
      paluses = ar.ReadBytes(256);

      //NOTE(adm244): reg: ABGR; mem: RGBA
      defaultPallete = ar.ReadArrayInt32(256);

      views_count = ar.ReadInt32();
      characters_count = ar.ReadInt32();
      player_character_id = ar.ReadInt32();
      total_score = ar.ReadInt32();
      inventory_items_count = ar.ReadInt16();
      dialogs_count = ar.ReadInt32();
      dialog_messages_count = ar.ReadInt32();
      fonts_count = ar.ReadInt32();
      color_depth = ar.ReadInt32(); // bytes per pixel
      target_win = ar.ReadInt32();
      dialog_bullet = ar.ReadInt32();
      hotdot = ar.ReadInt16(); // inventory item hotdot
      hotdot_outter = ar.ReadInt16();
      unique_id = ar.ReadInt32();
      guis_count = ar.ReadInt32();
      cursors_count = ar.ReadInt32();

      default_resolution = ar.ReadInt32();
      if ((dta_version >= 44) && (default_resolution == 8)) // 3.3.1, 8 - custom resolution
      {
        window_width = ar.ReadInt32();
        window_height = ar.ReadInt32();
      }

      default_lipsync_frame = ar.ReadInt32();
      inventory_hotdot_sprite = ar.ReadInt32();
      reserved = ar.ReadArrayInt32(17);
      global_messages = ar.ReadArrayInt32(500);

      load_dictionary = ar.ReadInt32();
      some_globalscript_value = ar.ReadInt32();
      some_chars_value = ar.ReadInt32();
      is_scriptcompiled = ar.ReadInt32();
    }

    public void WriteToStream(AGSAlignedStream ar, int dta_version)
    {
      ar.WriteFixedString(name, 50);
      ar.WriteArrayInt32(options);
      ar.WriteBytes(paluses);

      ar.WriteArrayInt32(defaultPallete);

      ar.WriteInt32(views_count);
      ar.WriteInt32(characters_count);
      ar.WriteInt32(player_character_id);
      ar.WriteInt32(total_score);
      ar.WriteInt16(inventory_items_count);
      ar.WriteInt32(dialogs_count);
      ar.WriteInt32(dialog_messages_count);
      ar.WriteInt32(fonts_count);
      ar.WriteInt32(color_depth);
      ar.WriteInt32(target_win);
      ar.WriteInt32(dialog_bullet);
      ar.WriteInt16(hotdot);
      ar.WriteInt16(hotdot_outter);
      ar.WriteInt32(unique_id);
      ar.WriteInt32(guis_count);
      ar.WriteInt32(cursors_count);

      ar.WriteInt32(default_resolution);
      if ((dta_version >= 44) && (default_resolution == 8)) // 3.3.1, 8 - custom resolution
      {
        ar.WriteInt32(window_width);
        ar.WriteInt32(window_height);
      }

      ar.WriteInt32(default_lipsync_frame);
      ar.WriteInt32(inventory_hotdot_sprite);
      ar.WriteArrayInt32(new Int32[17]);
      ar.WriteArrayInt32(global_messages);

      ar.WriteInt32(load_dictionary);
      ar.WriteInt32(some_globalscript_value);
      ar.WriteInt32(some_chars_value);
      ar.WriteInt32(is_scriptcompiled);
    }
  }
}
