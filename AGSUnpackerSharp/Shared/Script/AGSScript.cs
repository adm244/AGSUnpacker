using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using AGSUnpackerSharp.Extensions;

namespace AGSUnpackerSharp.Shared.Script
{
  public struct AGSScriptExport
  {
    public string Name;
    public Int32 Pointer;
  }

  public struct AGSScriptSection
  {
    public string Name;
    public Int32 Offset;
  }

  public struct AGSScriptFixup
  {
    public byte Type;
    public UInt32 Value;
  }

  public class AGSScript
  {
    private static readonly string HEAD_SIGNATURE = "SCOM";
    private static readonly UInt32 TAIL_SIGNATURE = 0xBEEFCAFE;

    public Int32 Version;
    public byte[] GlobalData;
    public Int32[] Code;
    public byte[] StringsBlob;
    public string[] Strings;
    public string[] Imports;
    public AGSScriptExport[] Exports;
    public AGSScriptSection[] Sections;
    public AGSScriptFixup[] Fixups;

    public AGSScript()
    {
      GlobalData = new byte[0];
      Code = new Int32[0];
      StringsBlob = new byte[0];
      Strings = new string[0];
      Imports = new string[0];
      Exports = new AGSScriptExport[0];
      Sections = new AGSScriptSection[0];
      Fixups = new AGSScriptFixup[0];
    }

    public void DumpInstructions(TextWriter writer)
    {
      //NOTE(adm244): pre-process fixups data
      byte[] fixups = new byte[Code.Length];
      for (int i = 0; i < Fixups.Length; ++i)
      {
        //NOTE(adm244): it looks like it's treated as a usual literal, so skip it
        if (Fixups[i].Type == (byte)AGSFixupType.DataData)
          continue;

        if (Fixups[i].Type == (byte)AGSFixupType.GlobalData)
        {
          //Debug.Assert(false, "GlobalData fixup type is not supported yet!");
          continue;
        }

        fixups[Fixups[i].Value] = Fixups[i].Type;
      }

      for (int ip = 0; ip < Code.Length; )
      {
        AGSInstruction instruction = AGSVirtualMachine.DisassembleInstruction(this, fixups, Code, ip);
        ip += instruction.ArgumentsCount + 1;

        writer.Write(instruction.Mnemonic);
        for (int arg = 0; arg < instruction.Arguments.Length; ++arg)
        {
          if (arg > 0)
            writer.Write(",");

          writer.Write(" {0}", instruction.Arguments[arg].ToString());
        }
        writer.WriteLine();
      }
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
      w.Write((UInt32)GlobalData.Length);
      w.Write((UInt32)Code.Length);

      //NOTE(adm244): AGSScript.stringsBlob and AGSScript.strings are NOT in sync!
      byte[] stringsBlob = ConvertStringsToBlob(Strings);
      w.Write((UInt32)stringsBlob.Length);

      // write main sections
      w.Write(GlobalData);
      w.WriteArrayInt32(Code);
      w.Write(stringsBlob);

      // write fixups section
      w.Write((Int32)Fixups.Length);
      for (int i = 0; i < Fixups.Length; ++i)
      {
        w.Write((byte)Fixups[i].Type);
      }
      for (int i = 0; i < Fixups.Length; ++i)
      {
        w.Write((UInt32)Fixups[i].Value);
      }

      // write imports section
      w.Write((Int32)Imports.Length);
      for (int i = 0; i < Imports.Length; ++i)
      {
        w.WriteNullTerminatedString(Imports[i], 300);
      }

      // write exports section
      w.Write((Int32)Exports.Length);
      for (int i = 0; i < Exports.Length; ++i)
      {
        w.WriteNullTerminatedString(Exports[i].Name, 300);
        w.Write((Int32)Exports[i].Pointer);
      }

      // write script sections
      if (version >= 83)
      {
        w.Write((Int32)Sections.Length);
        for (int i = 0; i < Sections.Length; ++i)
        {
          w.WriteNullTerminatedString(Sections[i].Name, 300);
          w.Write((Int32)Sections[i].Offset);
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

      Version = r.ReadInt32();

      // read section sizes
      Int32 globaldata_size = r.ReadInt32();
      Int32 code_size = r.ReadInt32();
      Int32 strings_size = r.ReadInt32();

      // parse global data section
      if (globaldata_size > 0)
      {
        GlobalData = r.ReadBytes(globaldata_size);
      }

      // parse code section
      if (code_size > 0)
      {
        Code = r.ReadArrayInt32(code_size);
      }

      // parse strings section
      if (strings_size > 0)
      {
        //NOTE(adm244): sequence of null terminated strings
        StringsBlob = r.ReadBytes(strings_size);
        Strings = AGSStringUtils.ConvertNullTerminatedSequence(StringsBlob);
      }

      // parse fixups section
      Int32 fixups_count = r.ReadInt32();
      Fixups = new AGSScriptFixup[fixups_count];
      for (int i = 0; i < fixups_count; ++i)
      {
        Fixups[i].Type = r.ReadByte();
      }
      for (int i = 0; i < fixups_count; ++i)
      {
        Fixups[i].Value = r.ReadUInt32();
      }

      // parse imports section
      Int32 imports_count = r.ReadInt32();
      Imports = new string[imports_count];
      for (int i = 0; i < imports_count; ++i)
      {
        Imports[i] = r.ReadNullTerminatedString(300);
      }

      // parse exports section
      Int32 exports_count = r.ReadInt32();
      Exports = new AGSScriptExport[exports_count];
      for (int i = 0; i < exports_count; ++i)
      {
        Exports[i].Name = r.ReadNullTerminatedString(300);
        Exports[i].Pointer = r.ReadInt32();
      }

      // parse script sections
      if (Version >= 83)
      {
        Int32 sections_count = r.ReadInt32();
        Sections = new AGSScriptSection[sections_count];
        for (int i = 0; i < sections_count; ++i)
        {
          Sections[i].Name = r.ReadNullTerminatedString(300);
          Sections[i].Offset = r.ReadInt32();
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
