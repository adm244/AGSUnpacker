using System;
using System.IO;

namespace AGSUnpacker.Lib
{
  public class AGSViewLoop
  {
    public Int32 Flags;
    public AGSViewLoopFrame[] Frames;

    public AGSViewLoop()
    {
      Flags = 0;
      Frames = new AGSViewLoopFrame[0];
    }

    public void LoadFromStream(BinaryReader r)
    {
      Int16 frames_count = r.ReadInt16();
      Frames = new AGSViewLoopFrame[frames_count];
      Flags = r.ReadInt32();

      AGSAlignedStream ar = new AGSAlignedStream(r);
      for (int i = 0; i < Frames.Length; ++i)
      {
        Frames[i] = new AGSViewLoopFrame();
        Frames[i].LoadFromStream(ar);
        ar.Reset();
      }
    }

    public void WriteToStream(BinaryWriter writer)
    {
      writer.Write((Int16)Frames.Length);
      writer.Write((Int32)Flags);

      AGSAlignedStream aw = new AGSAlignedStream(writer);
      for (int i = 0; i < Frames.Length; ++i)
      {
        Frames[i].WriteToStream(aw);
        aw.Reset();
      }
    }
  }
}
