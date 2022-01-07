using System;
using System.IO;

namespace AGSUnpacker.Lib.Game.View
{
  public class AGSView272
  {
    public Int16[] frames_count;
    public Int32[] loop_flags;
    public AGSViewLoopFrame[,] frames;
    public string scriptName;

    public AGSView272()
    {
      frames_count = new Int16[16];
      loop_flags = new Int32[16];
      frames = new AGSViewLoopFrame[16, 20];
      scriptName = string.Empty;
    }

    public void LoadFromStream(BinaryReader r)
    {
      Int16 loops_count = r.ReadInt16();
      for (int i = 0; i < frames_count.Length; ++i)
        frames_count[i] = r.ReadInt16();

      // fixed padding
      r.ReadInt16();

      for (int i = 0; i < loop_flags.Length; ++i)
        loop_flags[i] = r.ReadInt32();

      AGSAlignedStream ar = new AGSAlignedStream(r);
      for (int i = 0; i < 16; ++i)
      {
        for (int j = 0; j < 20; ++j)
        {
          frames[i, j] = new AGSViewLoopFrame();
          frames[i, j].LoadFromStream(ar);
        }
        ar.Reset();
      }
    }

    public void WriteToStream(BinaryWriter w)
    {
      throw new NotImplementedException();
    }
  }
}
