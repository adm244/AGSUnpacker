using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace AGSUnpackerSharp.Shared
{
  public struct AGSScriptExport
  {
    public string name;
    public Int32 pointer;
  }

  public struct AGSScriptSection
  {
    public string name;
    public Int32 offset;
  }

  public class AGSScript
  {
    private static readonly string HEAD_SIGNATURE = "SCOM";
    private static readonly UInt32 TAIL_SIGNATURE = 0xBEEFCAFE;

    public Int32[] code;
    public string[] strings;
    public string[] imports;
    public AGSScriptExport[] exports;
    public AGSScriptSection[] sections;

    public AGSScript()
    {
      code = new Int32[0];
      strings = new string[0];
      imports = new string[0];
      exports = new AGSScriptExport[0];
      sections = new AGSScriptSection[0];
    }

    public void LoadFromStream(BinaryReader r)
    {
      // verify signature
      char[] scom_sig = r.ReadChars(4);
      string scom_sig_string = new string(scom_sig);
      Debug.Assert(scom_sig_string == HEAD_SIGNATURE);

      Int32 version = r.ReadInt32();

      // read section sizes
      Int32 globaldata_size = r.ReadInt32();
      Int32 code_size = r.ReadInt32();
      Int32 strings_size = r.ReadInt32();

      // parse global data section
      if (globaldata_size > 0)
      {
        //NOTE(adm244): skip for now
        r.BaseStream.Seek(globaldata_size, SeekOrigin.Current);
      }

      // parse code section
      if (code_size > 0)
      {
        code = r.ReadArrayInt32(code_size);
      }

      // parse strings section
      if (strings_size > 0)
      {
        //NOTE(adm244): sequence of null terminated strings
        byte[] buffer = r.ReadBytes(strings_size);
        strings = AGSStringUtils.ConvertNullTerminatedSequence(buffer);
      }

      // parse fixups section
      Int32 fixups_count = r.ReadInt32();
      //NOTE(adm244): skip for now
      r.BaseStream.Seek(fixups_count, SeekOrigin.Current);
      r.BaseStream.Seek(fixups_count * sizeof(Int32), SeekOrigin.Current);

      // parse imports section
      Int32 imports_count = r.ReadInt32();
      imports = new string[imports_count];
      for (int i = 0; i < imports_count; ++i)
      {
        imports[i] = r.ReadNullTerminatedString(300);
      }

      // parse exports section
      Int32 exports_count = r.ReadInt32();
      exports = new AGSScriptExport[exports_count];
      for (int i = 0; i < exports_count; ++i)
      {
        exports[i].name = r.ReadNullTerminatedString(300);
        exports[i].pointer = r.ReadInt32();
      }

      // parse script sections
      if (version >= 83)
      {
        Int32 sections_count = r.ReadInt32();
        sections = new AGSScriptSection[sections_count];
        for (int i = 0; i < sections_count; ++i)
        {
          sections[i].name = r.ReadNullTerminatedString(300);
          sections[i].offset = r.ReadInt32();
        }
      }

      // verify tail signature
      UInt32 tail_sig = r.ReadUInt32();
      Debug.Assert(tail_sig == TAIL_SIGNATURE);
    }

    // disassembles script and dumps instructions in a file
    public void Disassemble(string targetpath)
    {
      /*
       * Instructions are stored as Int32 where highest byte stores instance id:
       *       (memory)           (register)
       *  [ AA  BB  CC  01 ]  [ 01  CC  BB  AA ]
       *   [  opcode  ][id]    [id][  opcode  ]
       * 
       * If script compiled in debug mode, it contains "linenumber" instructions
       * with a line number as an argument.
       * 
       * For 3.4 opcode is 0..74 inclusive.
       * 
       * Each instruction can have multiple arguments represented as Int32.
       * For 3.4 maximum arguments count is 3.
       * Argument may be either a numeric value or an offset\pointer.
       * Fixups table is used to determine the type of an argument.
       * 
       * Fixups types:
       *  0x0 : numerical literal
       *  GLOBAL_DATA = 0x1 : pointer?
       *  FUNCTION = 0x2 : offset from a start of code section (pc value)
       *  STRING = 0x3 : offset to strings null-terminated sequence (in bytes)
       *  IMPORT = 0x4 : index for imports array
       *  STACK = 0x5 : offset on the stack (in bytes)
       * 
       * 
       */
    }
  }
}
