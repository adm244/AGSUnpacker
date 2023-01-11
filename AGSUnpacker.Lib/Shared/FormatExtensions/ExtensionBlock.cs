using System;
using System.Diagnostics;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Shared.FormatExtensions
{
  public static class ExtensionBlock
  {
    public static BlockType ReadSingle(BinaryReader reader,
      Func<BinaryReader, string, long, bool> readData, Options options)
    {
      BlockType blockType = (BlockType) (options.HasFlag(Options.Id32)
        ? reader.ReadInt32() : reader.ReadByte());
      if (!Enum.IsDefined(blockType) || blockType < 0)
        return blockType;

      if (blockType == BlockType.EndOfFile)
        return BlockType.EndOfFile;

      string id = reader.ReadFixedCString(16);
      long size = options.HasFlag(Options.Size64)
        ? reader.ReadInt64() : reader.ReadInt32();

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

    public static void WriteSingle(BinaryWriter writer, string id,
      Func<BinaryWriter, string, bool> writeData, Options options)
    {
      if (options.HasFlag(Options.Id32))
        writer.Write((Int32)BlockType.Extension);
      else
        writer.Write((byte)BlockType.Extension);

      writer.WriteFixedString(id, 16);

      long blockStartPreSize = writer.BaseStream.Position;

      //NOTE(adm244): a placeholder for an actual value
      if (options.HasFlag(Options.Size64))
        writer.Write((UInt64)0xDEADBEEF_DEADBEEF);
      else
        writer.Write((UInt32)0xDEADBEEF);

      long blockStart = writer.BaseStream.Position;

      if (!writeData(writer, id))
        throw new InvalidDataException($"Cannot write extension block '{id}'!");

      long blockEnd = writer.BaseStream.Position;
      long blockLength = blockEnd - blockStart;

      writer.BaseStream.Seek(blockStartPreSize, SeekOrigin.Begin);

      if (options.HasFlag(Options.Size64))
        writer.Write((Int64)blockLength);
      else
        writer.Write((Int32)blockLength);

      writer.BaseStream.Seek(blockEnd, SeekOrigin.Begin);
    }

    public static void WriteEndOfFile(BinaryWriter writer, Options options)
    {
      if (options.HasFlag(Options.Id32))
        writer.Write((Int32)BlockType.EndOfFile);
      else
        writer.Write((byte)BlockType.EndOfFile);
    }

    public static bool ReadMultiple(BinaryReader reader,
      Func<BinaryReader, string, long, bool> readData, Options options)
    {
      bool result = true;

      while (true)
      {
        BlockType blockType = ReadSingle(reader, readData, options);

        if (!Enum.IsDefined(blockType) || blockType < 0)
          throw new InvalidDataException(
            $"Unknown extension block '{blockType}' encountered in game data!");

        if (blockType == BlockType.InvalidData)
          result = false;

        if (blockType == BlockType.EndOfFile)
          break;
      }

      return result;
    }

    public enum BlockType
    {
      InvalidData = -1,

      Extension = 0x00,
      EndOfFile = 0xFF
    }

    [Flags]
    public enum Options
    {
      Id8 = 0,
      Id32 = 1,

      Size32 = 0,
      Size64 = 2,
    }
  }
}
