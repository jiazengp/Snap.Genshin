using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DGP.Genshin.Control.Converter
{
    public class BooleanToStyleConverter : DependencyObject, IValueConverter
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
            DependencyProperty.Register("TrueStyle", typeof(Style), typeof(BooleanToStyleConverter), new PropertyMetadata(null));

        public Style FalseStyle
        {
            get => (Style)GetValue(FalseStyleProperty);

            set => SetValue(FalseStyleProperty, value);
        }
        public static readonly DependencyProperty FalseStyleProperty =
            DependencyProperty.Register("FalseStyle", typeof(Style), typeof(BooleanToStyleConverter), new PropertyMetadata(null));
    }
}