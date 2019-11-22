using System;
using System.IO;

namespace AGSUnpackerSharp.Shared.Interaction
{
  public class AGSInteractionValue
  {
    public byte type;
    public Int32 value;
    public Int32 extra;

    public AGSInteractionValue()
    {
      type = 0;
      value = 0;
      extra = 0;
    }

    public void LoadFromStream(BinaryReader r)
    {
      //NOTE(adm244): read and discard padding
      type = (byte)r.ReadInt32();
      value = r.ReadInt32();
      extra = r.ReadInt32();
    }

    public void WriteToStream(BinaryWriter w)
    {
      w.Write((Int32)type);
      w.Write((Int32)value);
      w.Write((Int32)extra);
    }
  }
}
