using System;

namespace AGSUnpacker.UI
{
  internal enum AppStatus
  {
    Ready,
    Busy,
    Loading
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
        case AppStatus.Loading:
          return "Loading...";

        default:
          throw new NotImplementedException();
      }
    }
  }
}
