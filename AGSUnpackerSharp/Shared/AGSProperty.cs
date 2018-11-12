using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace AGSUnpackerSharp.Shared
{
  public class AGSProperty
  {
    public string[] names;
    public string[] values;

    public AGSProperty()
    {
      names = new string[0];
      values = new string[0];
    }

    public void LoadFromStream(BinaryReader r)
    {
      Int32 version = r.ReadInt32();
      Debug.Assert(version == 1);

      Int32 count = r.ReadInt32();
      names = new string[count];
      values = new string[count];

      //TODO(adm244): test that on a real dta file
      for (int i = 0; i < count; ++i)
      {
        names[i] = r.ReadNullTerminatedString(200);
        values[i] = r.ReadNullTerminatedString(500);
      }
    }
  }
}
