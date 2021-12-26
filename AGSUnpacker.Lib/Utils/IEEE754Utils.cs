using System;

namespace AGSUnpacker.Utils
{
  public static class IEEE754Utils
  {
    public static float Int32BitsToFloat(int value)
    {
      return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
    }

    public static int FloatToInt32Bits(float value)
    {
      return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
    }
  }
}
