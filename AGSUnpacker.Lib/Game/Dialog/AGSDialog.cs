using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Game
{
  public class AGSDialog
  {
    public string scriptName;
    public string[] options;
    public Int32[] flags;
    public Int32 ptr;
    public Int16[] entry_points;
    public Int16 startup_entry_point;
    public Int16 code_size;
    public Int32 options_number;
    public Int32 topic_flags;
    
    public byte[] old_dialog_code;
    public string old_dialog_script;

    public AGSDialog()
    {
      scriptName = string.Empty;
      options = new string[0];
      flags = new Int32[0];
      ptr = 0;
      entry_points = Array.Empty<Int16>();
      startup_entry_point = 0;
      code_size = 0;
      options_number = 0;
      topic_flags = 0;

      old_dialog_code = new byte[0];
      old_dialog_script = string.Empty;
    }

    public void LoadFromStream(BinaryReader r)
    {
      options = new string[30];
      for (int i = 0; i < options.Length; ++i)
        options[i] = r.ReadFixedCString(150);

      flags = r.ReadArrayInt32(30);
      ptr = r.ReadInt32();
      entry_points = r.ReadArrayInt16(30);
      startup_entry_point = r.ReadInt16();
      code_size = r.ReadInt16();
      options_number = r.ReadInt32();
      topic_flags = r.ReadInt32();

      //NOTE(adm244): since options can contain garbage,
      // we zeroing everything that's not used to make it less confusing

      string[] validOptions = new string[options_number];
      for (int i = 0; i < validOptions.Length; ++i)
        validOptions[i] = options[i];

      options = new string[30];
      for (int i = 0; i < options.Length; ++i)
        options[i] = string.Empty;
      for (int i = 0; i < validOptions.Length; ++i)
        options[i] = validOptions[i];
    }

    public void WriteToStream(BinaryWriter writer)
    {
      for (int i = 0; i < options.Length; ++i)
        writer.WriteFixedString(options[i], 150);

      writer.WriteArrayInt32(flags);
      writer.Write((Int32)ptr);
      writer.WriteArrayInt16(entry_points);
      writer.Write((Int16)startup_entry_point);
      writer.Write((Int16)code_size);
      writer.Write((Int32)options_number);
      writer.Write((Int32)topic_flags);
    }
  }
}
