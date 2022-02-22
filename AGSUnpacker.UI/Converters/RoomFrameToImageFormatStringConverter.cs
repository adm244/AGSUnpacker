using System;
using System.Globalization;
using System.Windows.Data;

using AGSUnpacker.UI.Models.Room;

namespace AGSUnpacker.UI.Converters
{
  internal class RoomFrameToImageFormatStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is RoomFrame frame)
      {
        switch (frame.Source.Format.BitsPerPixel)
        {
          case 8:
            return "8bpp image (palette)";
          case 16:
            return "16bpp image (bgr565)";
          case 24:
            return "24bpp image (bgr24)";
          case 32:
            return "32bpp image (bgra32)";

          default:
            throw new NotImplementedException();
        }
      }

      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
