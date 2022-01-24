using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DGP.Genshin.Control.Converter
{
    /// <summary>
    /// 将字符形式的稀有度数字转换到 <see cref="SolidColorBrush"/>
    /// </summary>
    public class RankToStarSolidConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value switch
            {
                "1" => new SolidColorBrush(Color.FromRgb(114, 119, 139)),
                "2" => new SolidColorBrush(Color.FromRgb(42, 143, 114)),
                "3" => new SolidColorBrush(Color.FromRgb(81, 128, 203)),
                "4" => new SolidColorBrush(Color.FromRgb(161, 86, 224)),
                "5" => new SolidColorBrush(Color.FromRgb(188, 105, 50)),
                _ => null,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
