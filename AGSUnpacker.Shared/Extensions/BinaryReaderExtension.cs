using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

using AGSUnpacker.Shared.Utils;
using AGSUnpacker.Shared.Utils.Encryption;

namespace AGSUnpacker.Shared.Extensions
{
  internal static class BinaryReaderExtension
  {
    public static bool EOF(this BinaryReader reader)
    {
      return (reader.BaseStream.Position >= reader.BaseStream.Length);
    }

    public static string ReadCString(this BinaryReader reader)
    {
      if (reader.EOF())
        return string.Empty;

      return ReadCString(reader, AGSStringUtils.MaxCStringLength);
    }

    public static string ReadCString(this BinaryReader reader, int maxLength)
    {
      if (reader.EOF())
        return string.Empty;

      StringBuilder stringBuilder = new StringBuilder(maxLength / 4);

      for (int i = 0; i < maxLength; ++i)
      {
        char character = reader.ReadChar();
        if (character == 0)
          break;

        stringBuilder.Append(character);
      }

      return stringBuilder.ToString();
    }

    public static string ReadEncryptedCString(this BinaryReader reader)
    {
      Int32 length = reader.ReadInt32();
      if ((length < 0) || (length > AGSStringUtils.MaxCStringLength))
        throw new IndexOutOfRangeException();

      if (length == 0)
        return string.Empty;

      //NOTE(adm244): ASCII ReadChars is not reliable in this case since it replaces bytes > 0x7F
      // https://referencesource.microsoft.com/#mscorlib/system/text/asciiencoding.cs,879

      byte[] buffer = reader.ReadBytes(length);
      return AGSEncryption.DecryptAvis(buffer);
    }

    // TODO(adm244): verify that merging this won't break anything...
    public static string ReadFixedString(this BinaryReader reader, int length)
    {
      if (length < 1)
        return string.Empty;

      if (reader.EOF())
        return string.Empty;

      char[] buffer = reader.ReadChars(length);

      return new string(buffer);
    }

    public static string ReadFixedString(this BinaryReader reader, int length, Encoding encoding)
    {
      if (length < 1)
        return string.Empty;

      if (reader.EOF())
        return string.Empty;

      byte[] buffer = reader.ReadBytes(length);

      return encoding.GetString(buffer);
    }

    public static string ReadFixedCString(this BinaryReader reader, int length)
    {
      if (length < 1)
        return string.Empty;

      if (reader.EOF())
        return string.Empty;

      char[] buffer = reader.ReadChars(length);

      //NOTE(adm244): the reason why we use ConvertCString here is that we expect that
      // the string can be null-terminated (when it's actual length is less the the length specified)
      // Should we consider using a different method for this case??
      return AGSStringUtils.ConvertCString(buffer);
    }

    public static string ReadPrefixedString8(this BinaryReader reader)
    {
      if (reader.EOF())
        return string.Empty;

      byte length = reader.ReadByte();
      char[] buffer = reader.ReadChars(length);
      return new string(buffer);
    }

    public static string ReadPrefixedString32(this BinaryReader reader)
    {
      if (reader.EOF())
        return string.Empty;

      Int32 length = reader.ReadInt32();
      char[] buffer = reader.ReadChars(length);
      return new string(buffer);
    }

    public static Int16[] ReadArrayInt16(this BinaryReader reader, int count)
    {
      Int16[] values = new Int16[count];

      for (int i = 0; i < count; ++i)
        values[i] = reader.ReadInt16();

      return values;
    }

    public static Int32[] ReadArrayInt32(this BinaryReader reader, int count)
    {
      Int32[] values = new Int32[count];

      for (int i = 0; i < count; ++i)
        values[i] = reader.ReadInt32();

      return values;
    }

    // REWRITE(adm244): implement custom BinaryReader that's endianness-aware

    public static Int16 ReadInt16BE(this BinaryReader reader)
    {
      ReadOnlySpan<byte> bytes = reader.ReadBytes(sizeof(Int16));
      return BinaryPrimitives.ReadInt16BigEndian(bytes);
    }

    public static UInt16 ReadUInt16BE(this BinaryReader reader)
    {
      ReadOnlySpan<byte> bytes = reader.ReadBytes(sizeof(UInt16));
      return BinaryPrimitives.ReadUInt16BigEndian(bytes);
    }

    public static Int32 ReadInt32BE(this BinaryReader reader)
    {
      ReadOnlySpan<byte> bytes = reader.ReadBytes(sizeof(Int32));
      return BinaryPrimitives.ReadInt32BigEndian(bytes);
    }

    public static UInt32 ReadUInt32BE(this BinaryReader reader)
    {
      ReadOnlySpan<byte> bytes = reader.ReadBytes(sizeof(UInt32));
      return BinaryPrimitives.ReadUInt32BigEndian(bytes);
    }
  }
}
