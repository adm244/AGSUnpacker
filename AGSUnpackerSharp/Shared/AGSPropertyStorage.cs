using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using AGSUnpackerSharp.Extensions;

namespace AGSUnpackerSharp.Shared
{
  public class AGSPropertyStorage
  {
    public Int32 version;
    public string[] names;
    public string[] values;

    public AGSPropertyStorage()
    {
      names = new string[0];
      values = new string[0];
    }

    public void WriteToStream(BinaryWriter w, int version)
    {
      w.Write((Int32)version);

      Debug.Assert(names.Length == values.Length);

      w.Write((Int32)names.Length);
      for (int i = 0; i < names.Length; ++i)
      {
        if (version == 1)
        {
          w.WriteNullTerminatedString(names[i], 200);
          w.WriteNullTerminatedString(values[i], 500);
        }
        else
        {
          w.WritePrefixedString32(names[i]);
          w.WritePrefixedString32(values[i]);
        }
      }
    }

    public void LoadFromStream(BinaryReader r)
    {
      version = r.ReadInt32();
      Debug.Assert((version == 1) || (version == 2));

      Int32 count = r.ReadInt32();
      names = new string[count];
      values = new string[count];

      for (int i = 0; i < count; ++i)
      {
        if (version == 1)
        {
          names[i] = r.ReadNullTerminatedString(200);
          values[i] = r.ReadNullTerminatedString(500);
        }
        else
        {
          names[i] = r.ReadPrefixedString32();
          values[i] = r.ReadPrefixedString32();
        }
      }
    }
  }
}
