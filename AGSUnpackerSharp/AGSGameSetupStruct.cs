using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGSUnpackerSharp
{
  public struct AGSGameSetupStruct
  {
    public char[] name;
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
  }
}
