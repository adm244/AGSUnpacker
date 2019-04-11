using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AGSUnpackerSharp
{
  public static class BinaryReaderExtension
  {
    public static string ReadNullTerminatedString(this BinaryReader r)
    {
      return ReadNullTerminatedString(r, 5000000);
    }

    public static string ReadNullTerminatedString(this BinaryReader r, int maxLength)
    {
      StringBuilder sb = new StringBuilder(maxLength / 4);
      for (int i = 0; i < maxLength; ++i)
      {
        char symbol = r.ReadChar();
        if (symbol == 0) break;
        sb.Append(symbol);
      }

      return sb.ToString();
    }

    public static string ReadFixedString(this BinaryReader r, int length)
    {
      char[] buffer = r.ReadChars(length);
      StringBuilder sb = new StringBuilder(length);
      for (int i = 0; i < length; ++i)
      {
        if (buffer[i] == 0) break;
        sb.Append(buffer[i]);
      }

      return sb.ToString();
    }

    public static string ReadPrefixedString32(this BinaryReader r)
    {
      Int32 length = r.ReadInt32();
      char[] buffer = r.ReadChars(length);

      return new string(buffer);
    }

    public static Int16[] ReadArrayInt16(this BinaryReader r, int count)
    {
      Int16[] values = new Int16[count];
      for (int i = 0; i < count; ++i)
      {
        values[i] = r.ReadInt16();
      }

      return values;
    }

    public static Int32[] ReadArrayInt32(this BinaryReader r, int count)
    {
      Int32[] values = new Int32[count];
      for (int i = 0; i < count; ++i)
      {
        values[i] = r.ReadInt32();
      }

      return values;
    }
  }
}
