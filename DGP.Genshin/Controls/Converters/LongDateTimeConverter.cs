using System;
using System.Globalization;
using System.Windows.Data;

namespace DGP.Genshin.Controls.Converters
{
    public class LongDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DateTimeOffset.FromUnixTimeSeconds(long.Parse((string)value)).ToLocalTime().DateTime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}