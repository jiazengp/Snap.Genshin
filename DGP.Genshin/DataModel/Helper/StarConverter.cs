using System;
using System.Globalization;
using System.Windows.Data;

namespace DGP.Genshin.DataModel.Helper
{
    public class StarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return StarHelper.FromRank((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
