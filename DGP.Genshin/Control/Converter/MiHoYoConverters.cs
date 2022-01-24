using DGP.Genshin.DataModel.Helper;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DGP.Genshin.Control.Converter
{
    /// <summary>
    /// 将 英文元素名称 转换到 图标Url
    /// </summary>
    public class ElementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return ElementHelper.FromENGName((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 将 稀有度数字转换到 底图Url
    /// </summary>
    public class RarityConverter : IValueConverter
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
