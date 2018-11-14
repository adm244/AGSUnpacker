using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace AGSUnpackerSharp.Shared
{
  public class AGSPropertyStorage
  {
    public string[] names;
    public string[] values;

    public AGSPropertyStorage()
    {
      names = new string[0];
      values = new string[0];
    }

    public void LoadFromStream(BinaryReader r)
    {
      Int32 version = r.ReadInt32();
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
          Int32 strlen = r.ReadInt32();
          names[i] = r.ReadFixedString(strlen);

          strlen = r.ReadInt32();
          values[i] = r.ReadFixedString(strlen);
        }
      }
    }
  }
}
