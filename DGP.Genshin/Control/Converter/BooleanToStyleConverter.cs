using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DGP.Genshin.Control.Converter
{
    /// <summary>
    /// 根据 <see cref="bool"/> 的值选择对应的 <see cref="Style"/>
    /// </summary>
    public sealed class BooleanToStyleConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool boolean)
            {
                flag = boolean;
            }
            else if (value is bool?)
            {
                bool? flag2 = (bool?)value;
                flag = flag2.HasValue && flag2.Value;
            }

            return !flag ? FalseStyle : TrueStyle;
        }

        /// <summary>
        /// 不实现此方法
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public Style TrueStyle
        {
            get => (Style)GetValue(TrueStyleProperty);

            set => SetValue(TrueStyleProperty, value);
        }
        public static readonly DependencyProperty TrueStyleProperty =
            DependencyProperty.Register(nameof(TrueStyle), typeof(Style), typeof(BooleanToStyleConverter));

        public Style FalseStyle
        {
            get => (Style)GetValue(FalseStyleProperty);

            set => SetValue(FalseStyleProperty, value);
        }
        public static readonly DependencyProperty FalseStyleProperty =
            DependencyProperty.Register(nameof(FalseStyle), typeof(Style), typeof(BooleanToStyleConverter));
    }
}