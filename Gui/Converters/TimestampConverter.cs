using System;
using System.Globalization;
using System.Windows.Data;

namespace MicrowaveMonitor.Gui.Converters
{
    public class TimestampConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
            {
                if ((DateTime)value == DateTime.MinValue)
                    return "ACTIVE";
                else
                    return ((DateTime)value).ToString("dd.MM.yyyy HH:mm:ss");
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}