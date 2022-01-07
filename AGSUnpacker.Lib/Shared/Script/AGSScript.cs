using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using AGSUnpacker.Lib.Extensions;
//using AGSUnpackerSharp.Shared.Script.Deprecated;

namespace AGSUnpacker.Lib.Shared.Script
{
  public struct AGSScriptExport
  {
    public string Name;
    public ExportType Type;
    public Int32 Pointer;

    public enum ExportType
    {
      Unknown = 0,
      Function = 1,
      Variable = 2,
    }
  }

  public struct AGSScriptSection
  {
    public string Name;
    public Int32 Offset;
  }

  public enum AGSFixupType
  {
    Literal = 0,
    GlobalData = 1,
    Function = 2,
    String = 3,
    Import = 4,
    DataData = 5,
    Stack = 6,
  }

  public struct AGSScriptFixup
  {
    public AGSFixupType Type;
    public int Offset;
  }

  public class AGSScript
  {
    private static readonly string HEAD_SIGNATURE = "SCOM";
    private static readonly UInt32 TAIL_SIGNATURE = 0xBEEFCAFE;

    private string[] _strings;

    public Int32 Version;
    public byte[] GlobalData;
    public Int32[] Code;
    //public byte[] StringsBlob;
    public Hashtable StringsTable;
    public string[] Imports;
    public AGSScriptExport[] Exports;
    public AGSScriptSection[] Sections;
    public AGSScriptFixup[] Fixups;

    public AGSScript()
    {
      _strings = null;

      GlobalData = new byte[0];
      Code = new Int32[0];
      //StringsBlob = new byte[0];
      StringsTable = new Hashtable();
      Imports = new string[0];
      Exports = new AGSScriptExport[0];
      Sections = new AGSScriptSection[0];
      Fixups = new AGSScriptFixup[0];
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
      //byte[] stringsBlob = ConvertStringsToBlob(Strings);
      byte[] stringsBlob = ConvertStringsToBlob(Strings);
      w.Write((UInt32)stringsBlob.Length);

      // write main sections
      w.Write(GlobalData);
      w.WriteArrayInt32(Code);
      w.Write(stringsBlob);

      // write fixups section
      w.Write((Int32)Fixups.Length);
      for (int i = 0; i < Fixups.Length; ++i)
        w.Write((byte)Fixups[i].Type);

      for (int i = 0; i < Fixups.Length; ++i)
        w.Write((UInt32)Fixups[i].Offset);

      // write imports section
      w.Write((Int32)Imports.Length);
      for (int i = 0; i < Imports.Length; ++i)
        w.WriteFixedCString(Imports[i], 300);

      // write exports section
      w.Write((Int32)Exports.Length);
      for (int i = 0; i < Exports.Length; ++i)
      {
        w.WriteFixedCString(Exports[i].Name, 300);

        UInt32 value = (UInt32)Exports[i].Pointer;
        value |= ((uint)Exports[i].Type << 24);

        w.Write((UInt32)value);
      }

      // write script sections
      if (version >= 83) // ???
      {
        w.Write((Int32)Sections.Length);
        for (int i = 0; i < Sections.Length; ++i)
        {
          w.WriteFixedCString(Sections[i].Name, 300);
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
        //StringsBlob = r.ReadBytes(strings_size);
        //Strings = AGSStringUtils.ConvertNullTerminatedSequence(StringsBlob);

        byte[] stringsBlob = r.ReadBytes(strings_size);
        //Strings = AGSStringUtils.ConvertNullTerminatedSequence(stringsBlob);
        PopulateStringsTable(stringsBlob);
      }

      // parse fixups section
      Int32 fixups_count = r.ReadInt32();
      Fixups = new AGSScriptFixup[fixups_count];
      for (int i = 0; i < fixups_count; ++i)
      {
        Fixups[i].Type = (AGSFixupType)r.ReadByte();
      }
      for (int i = 0; i < fixups_count; ++i)
      {
        Fixups[i].Offset = r.ReadInt32();
      }

      // parse imports section
      Int32 imports_count = r.ReadInt32();
      Imports = new string[imports_count];
      for (int i = 0; i < imports_count; ++i)
      {
        Imports[i] = r.ReadFixedCString(300);
      }

      // parse exports section
      Int32 exports_count = r.ReadInt32();
      Exports = new AGSScriptExport[exports_count];
      for (int i = 0; i < exports_count; ++i)
      {
        Exports[i].Name = r.ReadFixedCString(300);
        //Exports[i].Pointer = r.ReadInt32();
        
        UInt32 data = r.ReadUInt32();
        Exports[i].Type = (AGSScriptExport.ExportType)(data >> 24);
        Exports[i].Pointer = (Int32)(data & 0x00FFFFFF);
      }

      // parse script sections
      if (Version >= 83) // ???
      {
        Int32 sections_count = r.ReadInt32();
        Sections = new AGSScriptSection[sections_count];
        for (int i = 0; i < sections_count; ++i)
        {
          Sections[i].Name = r.ReadFixedCString(300);
          Sections[i].Offset = r.ReadInt32();
        }
      }

      // verify tail signature
      UInt32 tail_sig = r.ReadUInt32();
      Debug.Assert(tail_sig == TAIL_SIGNATURE);
    }

    private unsafe void PopulateStringsTable(byte[] buffer)
    {
      int startpos = 0;
      for (int i = 0; i < buffer.Length; ++i)
      {
        if (buffer[i] == 0)
        {
          fixed (byte* p = &buffer[startpos])
          {
            string str = new string((sbyte*)p);
            StringsTable.Add(startpos, str);
          }
          startpos = (i + 1);
        }
      }
    }

    public string[] Strings
    {
      get
      {
        if (_strings == null)
        {
          //NOTE(adm244): .NET is a setup for a failure

          List<int> keys = new List<int>();
          foreach (int key in StringsTable.Keys)
            keys.Add(key);

          keys.Sort();

          _strings = new string[keys.Count];

          for (int i = 0; i < _strings.Length; ++i)
            _strings[i] = (string)StringsTable[keys[i]];
        }

        return _strings;
      }
    }

    // disassembles script and dumps instructions in a file
    //public void Disassemble(string targetpath)
    //{
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
       *  GLOBAL_DATA = 0x1 : offset into globaldata (in bytes)
       *  FUNCTION = 0x2 : offset from a start of code section (pc value ???)
       *  STRING = 0x3 : offset to strings null-terminated sequence (in bytes)
       *  IMPORT = 0x4 : index for imports array
       *  DATADATA = 0x5 : relative offset into globaldata stored in globaldata (in bytes)
       *  STACK = 0x6 : offset on the stack (in bytes)
       * 
       * 
       * 
       */

      /*for (int i = 0; i < Fixups.Length; ++i)
      {
        AGSScriptFixup fixup = Fixups[i];

        switch (fixup.Type)
        {
          case AGSFixupType.Literal:
            {
              int value = Code[fixup.Value];
            }
            break;

          case AGSFixupType.GlobalData:
            {
              int offset = Code[fixup.Value];
              int value = GlobalData[offset];
            }
            break;

          case AGSFixupType.Function:
            {
              int offset = Code[fixup.Value];
              int value = Code[offset];
            }
            break;

          case AGSFixupType.String:
            {
              int offset = Code[fixup.Value];
              string value = ReadCString(StringsBlob[offset]);
            }
            break;

          case AGSFixupType.Import:
            {
              int index = Code[fixup.Value];
              string value = Imports[index];
            }
            break;

          case AGSFixupType.DataData:
            {
              int offset = GlobalData[fixup.Value];
              int value = GlobalData[offset];
            }
            break;

          case AGSFixupType.Stack:
            {
              int offset = Code[fixup.Value];
              int value = Stack[offset];
            }
            break;

          default:
            throw new InvalidDataException();
        }
      }*/
    //}
  }
}
