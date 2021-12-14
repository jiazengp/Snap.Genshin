using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.Services.Settings;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DGP.Genshin.Controls.Converters
{
    /// <summary>
    /// 开发模式专用可见性转换器
    /// </summary>
    public class DevModeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.GetService<ISettingService>().GetOrDefault(Setting.IsDevMode, false) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
