using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AGSUnpackerSharp
{
  public static class AGSStringUtils
  {
    private const string password = "Avis Durgan";

    //FIX(adm244): don't modify the passed-in array, make a copy!
    public static unsafe string DecryptString(byte[] str)
    {
      int passlen = password.Length;
      for (int i = 0; i < str.Length; ++i)
      {
        str[i] -= (byte)password[i % passlen];
      }

      //NOTE(AdamJensen): all this nonsence just to get a pointer? I never asked for this.
      fixed (byte* p = &str[0])
      {
        return new string((sbyte*)(p));
      }
    }

    public static byte[] EncryptString(string str)
    {
      byte[] buffer = new byte[str.Length];
      for (int i = 0; i < str.Length; ++i)
      {
        buffer[i] = (byte)str[i];
      }

      int passlen = password.Length;
      for (int i = 0; i < str.Length; ++i)
      {
        buffer[i] += (byte)password[i % passlen];
      }

      return buffer;
    }

    public static string ReadEncryptedString(BinaryReader r)
    {
      Int32 length = r.ReadInt32();
      if ((length < 0) || (length > 5000000)) return null;

      //NOTE(adm244): ASCII ReadChars is not reliable in this case since it replaces bytes > 0x7F
      // https://referencesource.microsoft.com/#mscorlib/system/text/asciiencoding.cs,879
      byte[] buffer = r.ReadBytes(length);
      return DecryptString(buffer);
    }

    public static void WriteEncryptedString(BinaryWriter w, string str)
    {
      byte[] buffer = EncryptString(str);
      w.Write((Int32)buffer.Length);
      w.Write(buffer);
    }

    public static unsafe string[] ConvertNullTerminatedSequence(byte[] buffer)
    {
      List<string> strings = new List<string>();
      int startpos = 0;
      for (int i = 0; i < buffer.Length; ++i)
      {
        if (buffer[i] == 0)
        {
          fixed (byte* p = &buffer[startpos])
          {
            strings.Add(new string((sbyte*)p));
          }
          startpos = (i + 1);
        }
      }

      return strings.ToArray();
    }
  }
}
