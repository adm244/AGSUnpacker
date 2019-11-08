using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AGSUnpackerSharp
{
  public static class AGSStringUtils
  {
    private const string password = "Avis Durgan";
    private const string hisJibzle = "My\x1\xde\x4Jibzle";

    public static string DejibzleString(byte[] str)
    {
      int jibzlerIndex = 0;
      char[] buffer = new char[str.Length];

      int i;
      for (i = 0; i < str.Length; ++i)
      {
        byte dejibzledChar = (byte)(str[i] - (byte)hisJibzle[jibzlerIndex++]);
        if (dejibzledChar == 0)
          break;

        buffer[i] = (char)dejibzledChar;
        if (jibzlerIndex > 10)
          jibzlerIndex = 0;
      }

      return new string(buffer, 0, i);
    }

    //FIX(adm244): don't modify the passed-in array, make a copy!
    //TODO(adm244): is this correct???
    public static unsafe string DecryptString(byte[] str)
    {
      int passlen = password.Length;
      for (int i = 0; i < str.Length; ++i)
      {
        str[i] -= (byte)password[i % passlen];
      }

      //NOTE(AdamJensen): all this nonsence just to get a pointer? I never asked for this.
      fixed (byte* p = &str[0])
        return new string((sbyte*)(p));
    }

    public static byte[] EncryptString(string str)
    {
      byte[] buffer = new byte[str.Length];
      for (int i = 0; i < str.Length; ++i)
        buffer[i] = (byte)str[i];

      return EncryptString(buffer);
    }

    public static byte[] EncryptString(byte[] buffer)
    {
      int passlen = password.Length;
      for (int i = 0; i < buffer.Length; ++i)
        buffer[i] += (byte)password[i % passlen];

      return buffer;
    }

    public static string ToString(byte[] buffer)
    {
      Encoding encoding = Encoding.GetEncoding(1252);
      return encoding.GetString(buffer);
    }

    public static string ReadEncryptedString(BinaryReader r)
    {
      Int32 length = r.ReadInt32();
      if ((length <= 0) || (length > 5000000))
        throw new IndexOutOfRangeException();

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
