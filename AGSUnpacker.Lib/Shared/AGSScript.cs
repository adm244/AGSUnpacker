using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using AGSUnpacker.Shared.Extensions;
using AGSUnpacker.Shared.Utils;

namespace AGSUnpacker.Lib.Shared
{
  public class AGSScript
  {
    private static readonly string SignatureHead = "SCOM";
    private static readonly uint SignatureTail = 0xBEEFCAFE;

    public int Version;
    public byte[] GlobalData;
    public int[] Code;
    public string[] StringsStored;
    //public string[] StringsReferenced;
    public ReferencedString[] StringsReferenced;
    public string[] Imports;
    public Export[] Exports;
    public Section[] Sections;
    public Fixup[] Fixups;

    private byte[] _stringsBlob;

    public AGSScript()
    {
      GlobalData = new byte[0];
      Code = new int[0];
      StringsStored = new string[0];
      //StringsReferenced = new string[0];
      StringsReferenced = new ReferencedString[0];
      Imports = new string[0];
      Exports = new Export[0];
      Sections = new Section[0];
      Fixups = new Fixup[0];

      _stringsBlob = new byte[0];
    }

    public static AGSScript ReadFromFile(string filepath)
    {
      AGSScript script = new AGSScript();

      using FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
      using BinaryReader reader = new BinaryReader(stream, Encoding.Latin1);

      script.ReadFromStream(reader);

      return script;
    }

    public void WriteToFile(string filepath)
    {
      using (FileStream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
      {
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Latin1))
        {
          WriteToStream(writer);
        }
      }
    }

    public void ReadFromStream(BinaryReader reader)
    {
      string signatureHead = reader.ReadFixedCString(SignatureHead.Length);
      Debug.Assert(signatureHead == SignatureHead);

      Version = reader.ReadInt32();

      ReadMainSection(reader);
      ReadFixupsSection(reader);
      ReadImportsSection(reader);
      ReadExportsSection(reader);

      if (Version >= 83) // ???
        ReadScriptSections(reader);

      StringsReferenced = GetReferencedStrings(_stringsBlob);

      UInt32 signatureTail = reader.ReadUInt32();
      Debug.Assert(signatureTail == SignatureTail);
    }

    public void WriteToStream(BinaryWriter writer)
    {
      writer.Write(SignatureHead.ToCharArray());

      writer.Write((Int32)Version);

      WriteMainSection(writer);
      WriteFixupsSection(writer);
      WriteImportsSection(writer);
      WriteExportsSection(writer);

      if (Version >= 83) // ???
        WriteScriptSections(writer);

      writer.Write((UInt32)SignatureTail);
    }

    public struct ReferencedString
    {
      public string Text;
      public int Offset;
    }

    private ReferencedString[] GetReferencedStrings(byte[] stringsBlob)
    {
      var strings = new List<ReferencedString>();

      for (int i = 0; i < Fixups.Length; ++i)
      {
        if (Fixups[i].Type == Fixup.FixupType.String)
        {
          Debug.Assert(Fixups[i].Offset > 0);
          Debug.Assert(Fixups[i].Offset < Code.Length);

          int index = Code[Fixups[i].Offset];
          string stringReferenced = AGSStringUtils.ConvertCString(stringsBlob, index);
          
          strings.Add(new ReferencedString()
          {
            Text = stringReferenced,
            Offset = Fixups[i].Offset
          });
        }
      }

      return strings.ToArray();
    }

    private void ReadStringsSection(BinaryReader reader, int size)
    {
      _stringsBlob = reader.ReadBytes(size);
      StringsStored = AGSStringUtils.ConvertNullTerminatedSequence(_stringsBlob);
    }

    private void ReadMainSection(BinaryReader reader)
    {
      Int32 globalDataSize = reader.ReadInt32();
      Int32 codeSize = reader.ReadInt32();
      Int32 stringsSize = reader.ReadInt32();

      if (globalDataSize > 0)
        GlobalData = reader.ReadBytes(globalDataSize);

      if (codeSize > 0)
        Code = reader.ReadArrayInt32(codeSize);

      if (stringsSize > 0)
        ReadStringsSection(reader, stringsSize);
    }

    public void ExtractReferencedStrings(string filepath)
    {
      filepath = Path.GetFullPath(filepath);

      string directoryPath = Path.GetDirectoryName(filepath);
      if (!Directory.Exists(directoryPath))
        Directory.CreateDirectory(directoryPath);

      using (FileStream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
      {
        using (StreamWriter writer = new StreamWriter(stream, Encoding.Latin1))
        {
          for (int i = 0; i < StringsReferenced.Length; ++i)
          {
            writer.WriteLine(StringsReferenced[i].Text);
            writer.WriteLine();
          }
        }
      }
    }

    private byte[] WriteStringsBlobAndModifyCodeReferences()
    {
      using (MemoryStream buffer = new MemoryStream())
      {
        using (BinaryWriter writer = new BinaryWriter(buffer, Encoding.Latin1))
        {
          for (int i = 0; i < StringsReferenced.Length; ++i)
          {
            int offset = StringsReferenced[i].Offset;
            Code[offset] = (int)writer.BaseStream.Position;
            writer.WriteCString(StringsReferenced[i].Text);
          }
        }

        return buffer.ToArray();
      }
    }

    private void WriteMainSection(BinaryWriter writer)
    {
      //NOTE(adm244): consider writing only referenced strings
      //byte[] stringsBlob = AGSStringUtils.ConvertToNullTerminatedSequence(StringsStored);

      byte[] stringsBlob = WriteStringsBlobAndModifyCodeReferences();

      writer.Write((UInt32)GlobalData.Length);
      writer.Write((UInt32)Code.Length);
      writer.Write((UInt32)stringsBlob.Length);

      if (GlobalData.Length > 0)
        writer.Write(GlobalData);

      if (Code.Length > 0)
        writer.WriteArrayInt32(Code);

      if (stringsBlob.Length > 0)
        writer.Write(stringsBlob);
    }

    private void ReadFixupsSection(BinaryReader reader)
    {
      Int32 count = reader.ReadInt32();

      Fixups = new Fixup[count];
      for (int i = 0; i < count; ++i)
      {
        byte fixupTypeRaw = reader.ReadByte();
        if (!Enum.IsDefined(typeof(Fixup.FixupType), (int)fixupTypeRaw))
        {
          Debug.Assert(false, "AGSScript: Unknown fixup type encountered, assuming literal.");
          fixupTypeRaw = (byte)Fixup.FixupType.Literal;
        }

        Fixups[i].Type = (Fixup.FixupType)fixupTypeRaw;
      }

      for (int i = 0; i < count; ++i)
        Fixups[i].Offset = reader.ReadInt32();
    }

    private void WriteFixupsSection(BinaryWriter writer)
    {
      writer.Write((Int32)Fixups.Length);

      for (int i = 0; i < Fixups.Length; ++i)
        writer.Write((byte)Fixups[i].Type);

      for (int i = 0; i < Fixups.Length; ++i)
        writer.Write((UInt32)Fixups[i].Offset);
    }

    private void ReadImportsSection(BinaryReader reader)
    {
      Int32 count = reader.ReadInt32();

      Imports = new string[count];
      for (int i = 0; i < count; ++i)
        Imports[i] = reader.ReadCString(300);
    }

    private void WriteImportsSection(BinaryWriter writer)
    {
      writer.Write((Int32)Imports.Length);

      for (int i = 0; i < Imports.Length; ++i)
        writer.WriteCString(Imports[i], 300);
    }

    private void ReadExportsSection(BinaryReader reader)
    {
      Int32 count = reader.ReadInt32();

      Exports = new Export[count];
      for (int i = 0; i < count; ++i)
      {
        Exports[i].Name = reader.ReadCString(300);

        UInt32 pointerRaw = reader.ReadUInt32();
        Exports[i].Type = (Export.ExportType)(pointerRaw >> 24);
        Exports[i].Pointer = (Int32)(pointerRaw & 0x00FFFFFF);
      }
    }

    private void WriteExportsSection(BinaryWriter writer)
    {
      writer.Write((Int32)Exports.Length);

      for (int i = 0; i < Exports.Length; ++i)
      {
        writer.WriteCString(Exports[i].Name, 300);

        UInt32 pointerRaw = (UInt32)Exports[i].Pointer;
        pointerRaw |= ((uint)Exports[i].Type << 24);
        writer.Write((UInt32)pointerRaw);
      }
    }

    private void ReadScriptSections(BinaryReader reader)
    {
      Int32 count = reader.ReadInt32();

      Sections = new Section[count];
      for (int i = 0; i < count; ++i)
      {
        Sections[i].Name = reader.ReadCString(300);
        Sections[i].Offset = reader.ReadInt32();
      }
    }

    private void WriteScriptSections(BinaryWriter writer)
    {
      writer.Write((Int32)Sections.Length);

      for (int i = 0; i < Sections.Length; ++i)
      {
        writer.WriteCString(Sections[i].Name, 300);
        writer.Write((Int32)Sections[i].Offset);
      }
    }

    public struct Export
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

    public struct Section
    {
      public string Name;
      public Int32 Offset;
    }

    public struct Fixup
    {
      public FixupType Type;
      public int Offset;

      public enum FixupType
      {
        Literal = 0,
        GlobalData = 1,
        Function = 2,
        String = 3,
        Import = 4,
        DataData = 5,
        Stack = 6,
      }
    }
  }
}
