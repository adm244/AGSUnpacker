using System;

namespace AGSUnpacker.Lib
{
  public class AGSViewLoopFrame
  {
    public Int32 picture;
    public Int16 offset_x;
    public Int16 offset_y;
    public Int16 speed;
    public Int32 flags;
    public Int32 sound;
    public Int32 reserved1;
    public Int32 reserved2;

    public AGSViewLoopFrame()
    {
      picture = 0;
      offset_x = 0;
      offset_y = 0;
      speed = 0;
      flags = 0;
      sound = 0;
      reserved1 = 0;
      reserved2 = 0;
    }

    public void LoadFromStream(AGSAlignedStream ar)
    {
      picture = ar.ReadInt32();
      offset_x = ar.ReadInt16();
      offset_y = ar.ReadInt16();
      speed = ar.ReadInt16();
      flags = ar.ReadInt32();
      sound = ar.ReadInt32();
      reserved1 = ar.ReadInt32();
      reserved2 = ar.ReadInt32();
    }

    public void WriteToStream(AGSAlignedStream aw)
    {
      aw.WriteInt32(picture);
      aw.WriteInt16(offset_x);
      aw.WriteInt16(offset_y);
      aw.WriteInt16(speed);
      aw.WriteInt32(flags);
      aw.WriteInt32(sound);
      aw.WriteInt32(reserved1);
      aw.WriteInt32(reserved2);
    }
  }
}
