using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using AGSUnpackerSharp.Extensions;

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

  public struct AGSScriptFixup
  {
    public byte type;
    public UInt32 value;
  }

  public class AGSScript
  {
    private static readonly string HEAD_SIGNATURE = "SCOM";
    private static readonly UInt32 TAIL_SIGNATURE = 0xBEEFCAFE;

    public Int32 version;
    public byte[] globaldata;
    public Int32[] code;
    public string[] strings;
    public string[] imports;
    public AGSScriptExport[] exports;
    public AGSScriptSection[] sections;
    public AGSScriptFixup[] fixups;

    public AGSScript()
    {
      globaldata = new byte[0];
      code = new Int32[0];
      strings = new string[0];
      imports = new string[0];
      exports = new AGSScriptExport[0];
      sections = new AGSScriptSection[0];
      fixups = new AGSScriptFixup[0];
    }

    private byte[] ConvertStringsToBlob(string[] strings)
    {
      MemoryStream stream = new MemoryStream();
      for (int i = 0; i < strings.Length; ++i)
      {
        char[] chars = strings[i].ToCharArray();
        byte[] bytes = Encoding.GetEncoding(1252).GetBytes(chars);

        stream.Write(bytes, 0, bytes.Length);
        stream.WriteByte(0);
      }
      return stream.ToArray();
    }

    public void WriteToStream(BinaryWriter w, int version)
    {
      w.Write(HEAD_SIGNATURE.ToCharArray());
      w.Write((Int32)version);

      // write section sizes
      w.Write((UInt32)globaldata.Length);
      w.Write((UInt32)code.Length);

      byte[] stringsBlob = ConvertStringsToBlob(strings);
      w.Write((UInt32)stringsBlob.Length);

      // write main sections
      w.Write(globaldata);
      w.WriteArrayInt32(code);
      w.Write(stringsBlob);

      // write fixups section
      w.Write((Int32)fixups.Length);
      for (int i = 0; i < fixups.Length; ++i)
      {
        w.Write((byte)fixups[i].type);
      }
      for (int i = 0; i < fixups.Length; ++i)
      {
        w.Write((UInt32)fixups[i].value);
      }

      // write imports section
      w.Write((Int32)imports.Length);
      for (int i = 0; i < imports.Length; ++i)
      {
        w.WriteNullTerminatedString(imports[i], 300);
      }

      // write exports section
      w.Write((Int32)exports.Length);
      for (int i = 0; i < exports.Length; ++i)
      {
        w.WriteNullTerminatedString(exports[i].name, 300);
        w.Write((Int32)exports[i].pointer);
      }

      // write script sections
      if (version >= 83)
      {
        w.Write((Int32)sections.Length);
        for (int i = 0; i < sections.Length; ++i)
        {
          w.WriteNullTerminatedString(sections[i].name, 300);
          w.Write((Int32)sections[i].offset);
        }
      }

      // write tail signature
      w.Write((UInt32)TAIL_SIGNATURE);
    }

    public void LoadFromStream(BinaryReader r)
    {
      // verify signature
      char[] scom_sig = r.ReadChars(4);
      string scom_sig_string = new string(scom_sig);
      Debug.Assert(scom_sig_string == HEAD_SIGNATURE);

      version = r.ReadInt32();

      // read section sizes
      Int32 globaldata_size = r.ReadInt32();
      Int32 code_size = r.ReadInt32();
      Int32 strings_size = r.ReadInt32();

      // parse global data section
      if (globaldata_size > 0)
      {
        globaldata = r.ReadBytes(globaldata_size);
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
      fixups = new AGSScriptFixup[fixups_count];
      for (int i = 0; i < fixups_count; ++i)
      {
        fixups[i].type = r.ReadByte();
      }
      for (int i = 0; i < fixups_count; ++i)
      {
        fixups[i].value = r.ReadUInt32();
      }

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
