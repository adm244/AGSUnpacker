using System;
using System.Diagnostics;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Shared.FormatExtensions
{
  public static class ExtensionBlock
  {
    public static BlockType ReadSingle(BinaryReader reader, Func<BinaryReader, string, long, bool> readData)
    {
      BlockType blockType = (BlockType)reader.ReadByte();
      if (!Enum.IsDefined(blockType) || blockType < 0)
        return blockType;

      if (blockType == BlockType.EndOfFile)
        return BlockType.EndOfFile;

      string id = reader.ReadFixedCString(16);
      long size = reader.ReadInt64();

      long blockEnd = reader.BaseStream.Position + size;

      if (!readData(reader, id, size))
      {
        //NOTE(adm244): skip extension block if it cannot be parsed
        Debug.Assert(false, "Extension block cannot be processed!");
        reader.BaseStream.Seek(blockEnd, SeekOrigin.Begin);
        return BlockType.InvalidData;
      }

      return BlockType.Extension;
    }

    public static void WriteSingle(BinaryWriter writer, string id, Func<BinaryWriter, string, bool> writeData)
    {
      writer.Write((byte)BlockType.Extension);
      writer.WriteFixedString(id, 16);

      long blockStartPreSize = writer.BaseStream.Position;

      //NOTE(adm244): a placeholder for an actual value
      writer.Write((UInt64)0xDEADBEEF_DEADBEEF);

      long blockStart = writer.BaseStream.Position;

      if (!writeData(writer, id))
        throw new InvalidDataException($"Cannot write extension block '{id}'!");

      long blockEnd = writer.BaseStream.Position;
      long blockLength = blockEnd - blockStart;
      Debug.Assert(blockLength < Int64.MaxValue);

      writer.BaseStream.Seek(blockStartPreSize, SeekOrigin.Begin);
      writer.Write((Int64)blockLength);
      writer.BaseStream.Seek(blockEnd, SeekOrigin.Begin);
    }

    public enum BlockType
    {
      InvalidData = -1,

      Extension = 0x00,
      EndOfFile = 0xFF
    }
  }
}
