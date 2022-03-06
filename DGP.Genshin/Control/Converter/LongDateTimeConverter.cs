using System;
using System.Globalization;
using System.Windows.Data;

namespace DGP.Genshin.Control.Converter
{
    /// <summary>
    /// 将 Unix时间戳 转换到 <see cref="DateTime"/>
    /// </summary>
    public sealed class LongDateTimeConverter : IValueConverter
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