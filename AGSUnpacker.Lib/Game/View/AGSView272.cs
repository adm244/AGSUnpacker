using System;
using System.IO;

namespace AGSUnpacker.Lib.Game.View
{
  public class AGSView272
  {
    public Int16 loops_count;
    public Int16[] frames_count;
    public Int32[] loop_flags;
    public AGSViewLoopFrame[,] frames;
    public string scriptName;

    public Int16 i_guess_this_is_padding;

    public AGSView272()
    {
      loops_count = 0;
      frames_count = new Int16[16];
      loop_flags = new Int32[16];
      frames = new AGSViewLoopFrame[16, 20];
      scriptName = string.Empty;

      i_guess_this_is_padding = 0;
    }

    public void LoadFromStream(BinaryReader r)
    {
      // FIXME(adm244): why this is not being used anywhere?
      loops_count = r.ReadInt16();
      // FIXME(adm244): why an array named frames_count?
      for (int i = 0; i < frames_count.Length; ++i)
        frames_count[i] = r.ReadInt16();

      // FIXME(adm244): are we sure this is padding?
      // fixed padding
      i_guess_this_is_padding = r.ReadInt16();

      // FIXME(adm244): who wrote this shit anyways...

      for (int i = 0; i < loop_flags.Length; ++i)
        loop_flags[i] = r.ReadInt32();

      AGSAlignedStream ar = new AGSAlignedStream(r);
      // FIXME(adm244): are these numbers correct for all 2.x versions?
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

    public void WriteToStream(BinaryWriter writer)
    {
      writer.Write((Int16)loops_count);
      
      for (int i = 0; i < frames_count.Length; ++i)
        writer.Write((Int16)frames_count[i]);

      writer.Write((Int16)i_guess_this_is_padding);

      for (int i = 0; i < loop_flags.Length; ++i)
        writer.Write((Int32)loop_flags[i]);

      AGSAlignedStream aw = new AGSAlignedStream(writer);
      for (int i = 0; i < frames.GetLength(0); ++i)
      {
        for (int j = 0; j < frames.GetLength(1); ++j)
          frames[i, j].WriteToStream(aw);

        aw.Reset();
      }
    }
  }
}
