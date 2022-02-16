using System;
using System.IO;

using AGSUnpacker.Shared.Extensions;

namespace AGSUnpacker.Lib.Shared.Interaction
{
  public class AGSInteractionVariable
  {
    public string name;
    public byte type;
    public int value;

    public AGSInteractionVariable()
    {
      name = string.Empty;
      type = 0;
      value = 0;
    }

    public void LoadFromStream(BinaryReader r)
    {
      name = r.ReadFixedCString(23);
      type = r.ReadByte();
      value = r.ReadInt32();
    }

    public void WriteToStream(BinaryWriter w)
    {
      w.WriteFixedString(name, 23);
      w.Write((byte)type);
      w.Write((Int32)value);
    }
  }
}
