using System;
using System.IO;

using AGSUnpacker.Lib.Shared;
using AGSUnpacker.Shared.Utils;
using AGSUnpacker.Shared.Utils.Encryption;

namespace AGSUnpacker.Lib.Room
{
  public class AGSRoomScript
  {
    public AGSScript SCOM3;
    public string SourceCode;

    public AGSRoomScript()
    {
      SCOM3 = new AGSScript();
      SourceCode = string.Empty;
    }

    public void ReadSCOM3Block(BinaryReader reader, int roomVersion)
    {
      SCOM3 = new AGSScript();
      SCOM3.ReadFromStream(reader);
    }

    public void WriteSCOM3Block(BinaryWriter writer, int roomVersion)
    {
      SCOM3.WriteToStream(writer);
    }

    public void ReadSourceBlock(BinaryReader reader, int roomVersion)
    {
      int length = reader.ReadInt32();
      byte[] buffer = reader.ReadBytes(length);

      //NOTE(adm244): not a bug, it decrypts by encrypting
      buffer = AGSEncryption.EncryptAvisBuffer(buffer);
      SourceCode = AGSStringUtils.ConvertToString(buffer);
    }

    public void WriteSourceBlock(BinaryWriter writer, int roomVersion)
    {
      byte[] buffer = AGSStringUtils.GetASCIIBytes(SourceCode);

      //NOTE(adm244): not a bug, it encrypts by decrypting
      buffer = AGSEncryption.DecryptAvisBuffer(buffer);

      writer.Write((Int32)buffer.Length);
      writer.Write((byte[])buffer);
    }
  }
}
