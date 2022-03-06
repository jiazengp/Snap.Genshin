using DGP.Genshin.MiHoYoAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DGP.Genshin.Control.Converter
{
    /// <summary>
    /// 在XAML中支持移除Html内容
    /// </summary>
    public sealed class HtmlStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((JValue)value).Value?.ToString()?.RemoveXmlFormat();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}