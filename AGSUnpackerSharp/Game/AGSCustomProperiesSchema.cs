using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace AGSUnpackerSharp.Game
{
  public class AGSCustomProperiesSchema
  {
    public string Name;
    public string Description;
    public string DefaultValue;
    public Int32 Type;

    public AGSCustomProperiesSchema()
    {
      Name = string.Empty;
      Description = string.Empty;
      DefaultValue = string.Empty;
      Type = 0;
    }

    public void LoadFromStream(BinaryReader r)
    {
      Int32 version = r.ReadInt32();
      Debug.Assert((version == 1) || (version == 2));

      Int32 count = r.ReadInt32();
      for (int i = 0; i < count; ++i)
      {
        if (version == 1)
        {
          Name = r.ReadCString(20);
          Description = r.ReadCString(100);
          DefaultValue = r.ReadCString(500);
          Type = r.ReadInt32();
        }
        else
        {
          Name = r.ReadPrefixedString32();
          Type = r.ReadInt32();
          Description = r.ReadPrefixedString32();
          DefaultValue = r.ReadPrefixedString32();
        }
      }
    }
  }
}
