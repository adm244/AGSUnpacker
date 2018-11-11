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

  public struct AGSScript
  {
    private static readonly string HEAD_SIGNATURE = "SCOM";
    private static readonly UInt32 TAIL_SIGNATURE = 0xBEEFCAFE;

    public Int32[] code;
    public string[] strings;
    public string[] imports;
    public AGSScriptExport[] exports;
    public AGSScriptSection[] sections;

    public void LoadFromStream(BinaryReader r)
    {
      // verify signature
      char[] scom_sig = r.ReadChars(4);
      string scom_sig_string = new string(scom_sig);
      Debug.Assert(scom_sig_string == HEAD_SIGNATURE);

      Int32 version = r.ReadInt32();
      //Debug.Assert(r.BaseStream.Position == 0x8A51);

      // read section sizes
      Int32 globaldata_size = r.ReadInt32();
      Int32 code_size = r.ReadInt32();
      Int32 strings_size = r.ReadInt32();
      //Debug.Assert(r.BaseStream.Position == 0x8A5D);

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
      //Debug.Assert(r.BaseStream.Position == 0xFD79);

      // parse strings section
      if (strings_size > 0)
      {
        //NOTE(adm244): sequence of null terminated strings
        byte[] buffer = r.ReadBytes(strings_size);
        //Debug.Assert(r.BaseStream.Position == 0xFFF2);

        strings = AGSUtils.ConvertNullTerminatedSequence(buffer);
      }

      // parse fixups section
      Int32 fixups_count = r.ReadInt32();
      //NOTE(adm244): skip for now
      r.BaseStream.Seek(fixups_count, SeekOrigin.Current);
      //Debug.Assert(r.BaseStream.Position == 0x1017F);

      //NOTE(adm244): skip for now
      r.BaseStream.Seek(fixups_count * sizeof(Int32), SeekOrigin.Current);
      //Debug.Assert(r.BaseStream.Position == 0x107A3);

      // parse imports section
      Int32 imports_count = r.ReadInt32();
      imports = new string[imports_count];
      for (int i = 0; i < imports_count; ++i)
      {
        imports[i] = r.ReadNullTerminatedString(300);
      }
      //Debug.Assert(r.BaseStream.Position == 0x11115);

      // parse exports section
      Int32 exports_count = r.ReadInt32();
      exports = new AGSScriptExport[exports_count];
      for (int i = 0; i < exports_count; ++i)
      {
        exports[i].name = r.ReadNullTerminatedString(300);
        exports[i].pointer = r.ReadInt32();
      }
      //Debug.Assert(r.BaseStream.Position == 0x114F8);

      // parse script sections
      Int32 sections_count = r.ReadInt32();
      sections = new AGSScriptSection[sections_count];
      for (int i = 0; i < sections_count; ++i)
      {
        sections[i].name = r.ReadNullTerminatedString(300);
        sections[i].offset = r.ReadInt32();
      }
      //Debug.Assert(r.BaseStream.Position == 0x11511);

      // verify tail signature
      UInt32 tail_sig = r.ReadUInt32();
      Debug.Assert(tail_sig == TAIL_SIGNATURE);
    }
  }
}
