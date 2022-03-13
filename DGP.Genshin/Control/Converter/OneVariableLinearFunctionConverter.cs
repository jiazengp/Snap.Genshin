using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DGP.Genshin.Control.Converter
{
    public sealed class OneVariableLinearFunctionConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return A * (double)value + B;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public double A
        {
            get => (double)GetValue(AProperty);
            set => SetValue(AProperty, value);
        }
        public static readonly DependencyProperty AProperty =
            DependencyProperty.Register("A", typeof(double), typeof(OneVariableLinearFunctionConverter), new PropertyMetadata(0D));

        public double B
        {
            get => (double)GetValue(BProperty);
            set => SetValue(BProperty, value);
        }
        public static readonly DependencyProperty BProperty =
            DependencyProperty.Register("B", typeof(double), typeof(OneVariableLinearFunctionConverter), new PropertyMetadata(0D));
    }
}