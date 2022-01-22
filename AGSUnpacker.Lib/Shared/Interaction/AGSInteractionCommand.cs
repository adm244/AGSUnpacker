using System;
using System.IO;

namespace AGSUnpacker.Lib.Shared.Interaction
{
  public class AGSInteractionCommand
  {
    private readonly static int MAX_ACTION_ARGS = 5;

    public AGSInteractionValue[] args;
    public Int32 type;
    public AGSInteractionCommandsList children;
    public AGSInteractionCommandsList parent;

    public AGSInteractionCommand()
    {
      args = new AGSInteractionValue[MAX_ACTION_ARGS];
      type = 0;
      children = null;
      parent = null;
    }

    public void LoadFromStream(BinaryReader r)
    {
      Int32 vtable = r.ReadInt32();
      type = r.ReadInt32();

      for (int i = 0; i < args.Length; ++i)
      {
        args[i] = new AGSInteractionValue();
        args[i].LoadFromStream(r);
      }

      children = r.ReadInt32() == 0 ? null : new AGSInteractionCommandsList();
      parent = r.ReadInt32() == 0 ? null : new AGSInteractionCommandsList();
    }

    public void WriteToStream(BinaryWriter w)
    {
      w.Write((Int32)0);
      w.Write((Int32)type);

      for (int i = 0; i < args.Length; ++i)
        args[i].WriteToStream(w);

      w.Write((Int32)(children == null ? 0 : 1));
      w.Write((Int32)(parent == null ? 0 : 1));
    }
  }
}
