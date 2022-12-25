using System;

namespace AGSUnpacker.Shared.Utils
{
  internal static class Utils
  {
    public static int Remap(int max, int value, int newMax)
    {
      return (int)Math.Round(Remap((double)max, value, newMax));
    }

    public static double Remap(double max, double value, double newMax)
    {
      return (value / max) * newMax;
    }
  }
}
