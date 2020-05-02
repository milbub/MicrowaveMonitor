using System;
using System.Globalization;
using System.Windows.Data;

namespace MicrowaveMonitor.Gui.Converters
{
    public class VersionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var val = value.ToString();
                if (val.Equals("v3"))
                    return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}