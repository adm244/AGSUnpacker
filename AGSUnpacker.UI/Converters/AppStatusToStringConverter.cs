using System;
using System.Globalization;
using System.Windows.Data;

namespace AGSUnpacker.UI.Converters
{
  internal class AppStatusToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is AppStatus status)
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

      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
