using System;

namespace AGSUnpackerSharp.Graphics
{
  public class LZWImage
  {
    public Int32 picture_maxsize;
    public Int32 picture_data_size;
    public byte[] rawBackground;

    public LZWImage(Int32 maxSize, Int32 dataSize, byte[] rawData)
    {
      picture_maxsize = maxSize;
      picture_data_size = dataSize;
      rawBackground = rawData;
    }
  }
}
