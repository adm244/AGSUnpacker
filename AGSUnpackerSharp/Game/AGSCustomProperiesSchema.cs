using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace AGSUnpackerSharp.Game
{
  public class AGSCustomProperiesSchema
  {
    public AGSCustomProperiesSchema()
    {
      //NOTE(adm244): empty for now
    }

    public void LoadFromStream(BinaryReader r)
    {
      Int32 version = r.ReadInt32();
      Debug.Assert(version == 1);

      Int32 count = r.ReadInt32();
      for (int i = 0; i < count; ++i)
      {
        //TODO(adm244): parse schema
      }
    }
  }
}
