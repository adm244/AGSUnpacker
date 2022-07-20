using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AGSUnpacker.Shared.Utils
{
  internal static class AGSStringUtils
  {
    //RANT(adm244): 50.000.000 sounds like a non-sense, but so is AGS being a good engine
    public static readonly int MaxCStringLength = 5000000;

    public static readonly Encoding Encoding = Encoding.Latin1;

    public static int GetCStringLength(byte[] buffer, int index)
    {
      int i = 0;

      while (buffer[index + i] != 0)
      {
        if (i >= MaxCStringLength)
          break;

        ++i;

        if (i >= buffer.Length)
          throw new ArgumentException("Buffer does not contain a c-string");
      }

      return i;
    }

    public static unsafe string ConvertCString(byte[] buffer, int index)
    {
      int length = GetCStringLength(buffer, index);
      fixed (byte* p = &buffer[index])
        return new string((sbyte*)p, 0, length, Encoding);
    }

    public static unsafe string ConvertCString(byte[] buffer)
    {
      return ConvertCString(buffer, 0);
    }

    public static unsafe string ConvertCString(char[] buffer)
    {
      fixed (char* p = &buffer[0])
        return new string(p);
    }

    public static unsafe string ConvertCStringMaybe(byte[] buffer)
    {
      char[] characters = Encoding.GetChars(buffer);
      fixed (char* p = &characters[0])
        return new string(p);
    }

    public static string ConvertToString(byte[] buffer)
    {
      Debug.Assert(buffer != null);

      return Encoding.GetString(buffer);
    }

    // REDO(adm244): use Encoding to convert string to byte[] instead
    public static byte[] GetASCIIBytes(string text)
    {
      byte[] buffer = new byte[text.Length];

      for (int i = 0; i < text.Length; ++i)
        buffer[i] = (byte)text[i];

      return buffer;
    }

    public static unsafe string[] ConvertNullTerminatedSequence(byte[] buffer)
    {
      List<string> strings = new List<string>();

      int startpos = 0;
      for (int i = 0; i < buffer.Length; ++i)
      {
        if (buffer[i] == 0)
        {
          string substring = ConvertCString(buffer, startpos);
          strings.Add(substring);

          startpos = (i + 1);
        }
      }

      return strings.ToArray();
    }

    public static byte[] ConvertToNullTerminatedSequence(string[] strings)
    {
      using (MemoryStream stream = new MemoryStream())
      {
        for (int i = 0; i < strings.Length; ++i)
        {
          // TODO(adm244): implement a method to convert "string" into "byte[]"
          byte[] buffer = GetASCIIBytes(strings[i]);
          stream.Write(buffer, 0, buffer.Length);
          stream.WriteByte(0);
        }

        return stream.ToArray();
      }
    }
  }
}
