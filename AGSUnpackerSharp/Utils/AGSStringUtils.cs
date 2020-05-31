using System.Collections.Generic;
using System.IO;

namespace AGSUnpackerSharp
{
  public static class AGSStringUtils
  {
    //RANT(adm244): 50.000.000 sounds like a non-sense, but so is AGS being a good engine
    public static readonly int MaxCStringLength = 5000000;

    public static unsafe string ConvertCString(byte[] buffer, int index)
    {
      fixed (byte* p = &buffer[index])
        return new string((sbyte *)p);
    }

    public static unsafe string ConvertCString(byte[] buffer)
    {
      fixed (byte* p = &buffer[0])
        return new string((sbyte *)p);
    }

    public static unsafe string ConvertCString(char[] buffer)
    {
      fixed (char* p = &buffer[0])
        return new string(p);
    }

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
          byte[] buffer = GetASCIIBytes(strings[i]);
          stream.Write(buffer, 0, buffer.Length);
          stream.WriteByte(0);
        }

        return stream.ToArray();
      }
    }
  }
}
