using System;
using System.Globalization;
using System.Windows.Data;

namespace DGP.Genshin.Control.Converter
{
    public class BooleanRevertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}