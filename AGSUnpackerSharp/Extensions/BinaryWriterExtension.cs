using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AGSUnpackerSharp.Extensions
{
  public static class BinaryWriterExtension
  {
    public static void WriteNullTerminatedString(this BinaryWriter w, string str, int maxLength)
    {
      char[] bytes = new char[maxLength];
      for (int i = 0; i < str.Length; ++i)
      {
        bytes[i] = str[i];
      }
      w.Write(bytes);
    }
  }
}
