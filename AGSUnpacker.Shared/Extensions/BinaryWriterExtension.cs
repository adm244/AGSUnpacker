using System;
using System.IO;

using AGSUnpacker.Shared.Utils;
using AGSUnpacker.Shared.Utils.Encryption;

namespace AGSUnpacker.Shared.Extensions
{
  internal static class BinaryWriterExtension
  {
    /// <summary>
    /// Writes size difference between specified and current positions.
    /// Doesn't include position itself.
    /// </summary>
    /// <param name="writer">BinaryWriter</param>
    /// <param name="position">Position to write at</param>
    public static void FixInt32(this BinaryWriter writer, long position)
    {
      long currentPosition = writer.BaseStream.Position;
      writer.BaseStream.Seek(position, SeekOrigin.Begin);

      writer.Write((Int32)(currentPosition - position - sizeof (Int32)));

      writer.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
    }

    public static void WriteCString(this BinaryWriter writer, string text)
    {
      WriteCString(writer, text, AGSStringUtils.MaxCStringLength);
    }

    public static void WriteCString(this BinaryWriter writer, string text, int lengthMax)
    {
      byte[] buffer = new byte[lengthMax];
      int length = 0;

      for (int i = 0; i < text.Length; ++i)
      {
        if (i == lengthMax)
          break;

        buffer[i] = (byte)text[i];
        ++length;
      }

      writer.Write((byte[])buffer, 0, length);

      //NOTE(adm244): if string length exeeds maxLength it shouldn't be null-terminated
      if (length < lengthMax)
        writer.Write((byte)0);
    }

    // FIXME(adm244): this is NOT a CString, it's PrefixString32 !!!
    public static void WriteEncryptedCString(this BinaryWriter writer, string text)
    {
      byte[] buffer = AGSEncryption.EncryptAvis(text);
      writer.Write((Int32)buffer.Length);
      writer.Write((byte[])buffer);
    }

    public static void WriteFixedString(this BinaryWriter writer, string text, int length)
    {
      char[] buffer = new char[length];

      for (int i = 0; i < text.Length; ++i)
        buffer[i] = text[i];

      writer.Write((char[])buffer);
    }

    //FIXME(adm244): inconsistency in naming, this shouldn't add extra byte(!)
    public static void WriteFixedCString(this BinaryWriter writer, string text, int length)
    {
      char[] buffer = new char[length + 1];

      for (int i = 0; i < text.Length; ++i)
        buffer[i] = text[i];

      //NOTE(adm244): don't trust microsoft to have it initialized to 0
      buffer[length] = (char)0;

      writer.Write((char[])buffer);
    }

    public static void WritePrefixedString8(this BinaryWriter writer, string text)
    {
      if (text.Length > byte.MaxValue)
        throw new InvalidOperationException($"Text length is too big to fit 8-bit prefix: {text.Length}");

      writer.Write((byte)text.Length);

      char[] buffer = text.ToCharArray();
      writer.Write((char[])buffer);
    }

    public static void WritePrefixedString32(this BinaryWriter writer, string text)
    {
      writer.Write((Int32)text.Length);

      char[] buffer = text.ToCharArray();
      writer.Write((char[])buffer);
    }

    public static void WriteArrayInt16(this BinaryWriter writer, Int16[] values)
    {
      for (int i = 0; i < values.Length; ++i)
        writer.Write((Int16)values[i]);
    }

    public static void WriteArrayInt32(this BinaryWriter writer, int[] values)
    {
      for (int i = 0; i < values.Length; ++i)
        writer.Write(values[i]);
    }
  }
}
