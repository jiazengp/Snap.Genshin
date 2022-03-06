using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DGP.Genshin.Control.Converter
{
    public sealed class PercentageToHeightConverter : DependencyObject, IValueConverter
    {
        public double TargetWidth
        {
            get => (double)GetValue(TargetWidthProperty);

            set => SetValue(TargetWidthProperty, value);
        }
        public static readonly DependencyProperty TargetWidthProperty =
            DependencyProperty.Register(
                nameof(TargetWidth), 
                typeof(double), 
                typeof(PercentageToHeightConverter), 
                new PropertyMetadata(1080D));

        public double TargetHeight
        {
            get => (double)GetValue(TargetHeightProperty);

            set => SetValue(TargetHeightProperty, value);
        }
        public static readonly DependencyProperty TargetHeightProperty =
            DependencyProperty.Register(
                nameof(TargetHeight), 
                typeof(double), 
                typeof(PercentageToHeightConverter), 
                new PropertyMetadata(390D));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value * (TargetHeight / TargetWidth);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}