using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AGSUnpackerSharp.Game
{
  public class AGSAudioType
  {
    public Int32 id;
    public Int32 channels;
    public Int32 volume_dumping;
    public Int32 crossfade_speed;
    public Int32 reserved1;

    public AGSAudioType()
    {
      id = 0;
      channels = 0;
      volume_dumping = 0;
      crossfade_speed = 0;
      reserved1 = 0;
    }

    public void LoadFromStream(BinaryReader r)
    {
      id = r.ReadInt32();
      channels = r.ReadInt32();
      volume_dumping = r.ReadInt32();
      crossfade_speed = r.ReadInt32();
      reserved1 = r.ReadInt32();
    }
  }
}
