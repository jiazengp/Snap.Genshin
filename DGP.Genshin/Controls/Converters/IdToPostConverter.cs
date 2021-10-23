using System;
using System.Globalization;
using System.Windows.Data;

namespace DGP.Genshin.Controls.Converters
{
    /// <summary>
    /// 导航到网站的支持
    /// </summary>
    public class IdToPostConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Uri($@"https://bbs.mihoyo.com/ys/article/{value}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
