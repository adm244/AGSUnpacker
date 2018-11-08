using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGSUnpackerSharp
{
  public struct AGSViewLoopFrame
  {
    public Int32 picture;
    public Int16 offset_x;
    public Int16 offset_y;
    public Int16 speed;
    public Int32 flags;
    public Int32 sound;
    public Int32 reserved1;
    public Int32 reserved2;
  }
}
