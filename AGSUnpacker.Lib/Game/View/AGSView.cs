using System;
using System.IO;

namespace AGSUnpacker.Lib
{
  public class AGSView
  {
    public AGSViewLoop[] Loops;
    public string scriptName;

    public AGSView()
    {
      Loops = new AGSViewLoop[0];
    }

    public void LoadFromStream(BinaryReader r)
    {
      Int16 loop_count = r.ReadInt16();
      Loops = new AGSViewLoop[loop_count];
      for (int i = 0; i < Loops.Length; ++i)
      {
        Loops[i] = new AGSViewLoop();
        Loops[i].LoadFromStream(r);
      }
    }

    public void WriteToStream(BinaryWriter writer)
    {
      writer.Write((Int16)Loops.Length);
      for (int i = 0; i < Loops.Length; ++i)
        Loops[i].WriteToStream(writer);
    }
  }
}
