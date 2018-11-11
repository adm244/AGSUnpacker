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
      StringBuilder sb = new StringBuilder(maxLength);
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
