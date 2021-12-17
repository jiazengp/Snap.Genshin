using System;
using System.Globalization;
using System.Windows.Data;

namespace DGP.Genshin.DataModels.Helpers
{
    public class StarSolidConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return StarHelper.ToSolid((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
