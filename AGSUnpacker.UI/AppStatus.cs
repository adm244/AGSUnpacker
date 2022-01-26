using System;

namespace AGSUnpacker.UI
{
  internal enum AppStatus
  {
    Ready,
    Busy
  }

  internal static class AppStatusExtension
  {
    public static string AsString(this AppStatus status)
    {
      switch (status)
      {
        case AppStatus.Ready:
          return "Ready.";
        case AppStatus.Busy:
          return "Working...";

        default:
          throw new NotImplementedException();
      }
    }
  }
}
