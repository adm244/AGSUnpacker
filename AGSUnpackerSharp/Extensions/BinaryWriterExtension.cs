using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AGSUnpackerSharp.Extensions
{
  public static class BinaryWriterExtension
  {
    public static void WriteNullTerminatedString(this BinaryWriter w, string str)
    {
      WriteNullTerminatedString(w, str, 5000000);
    }

    public static void WriteNullTerminatedString(this BinaryWriter w, string str, int maxLength)
    {
      byte[] buffer = new byte[maxLength];
      int length = 0;

      for (int i = 0; i < str.Length; ++i)
      {
        if (i == maxLength)
          break;

        buffer[i] = (byte)str[i];
        ++length;
      }

      w.Write(buffer, 0, length);
      //NOTE(adm244): if string length exeeds maxLength it shouldn't be null-terminated
      if (length < maxLength)
        w.Write((byte)0);
    }

    public static void WriteFixedString(this BinaryWriter w, string str, int length)
    {
      byte[] bytes = new byte[length];
      for (int i = 0; i < str.Length; ++i)
      {
        bytes[i] = (byte)str[i];
      }

      w.Write(bytes);
    }

    public static void WriteArrayInt32(this BinaryWriter w, int[] values)
    {
      for (int i = 0; i < values.Length; ++i)
      {
        w.Write(values[i]);
      }
    }

    public static void WritePrefixedString32(this BinaryWriter w, string str)
    {
      w.Write((Int32)str.Length);
      w.Write(str.ToCharArray());
    }
  }
}
