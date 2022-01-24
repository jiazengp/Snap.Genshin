using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DGP.Genshin.Control.Converter
{
    public class PercentageToHeightConverter : DependencyObject, IValueConverter
    {
        public double TargetWidth
        {
            get { return (double)GetValue(TargetWidthProperty); }
            set { SetValue(TargetWidthProperty, value); }
        }
        public static readonly DependencyProperty TargetWidthProperty =
            DependencyProperty.Register("TargetWidth", typeof(double), typeof(PercentageToHeightConverter), new PropertyMetadata(1080D));

        public double TargetHeight
        {
            get { return (double)GetValue(TargetHeightProperty); }
            set { SetValue(TargetHeightProperty, value); }
        }
        public static readonly DependencyProperty TargetHeightProperty =
            DependencyProperty.Register("TargetHeight", typeof(double), typeof(PercentageToHeightConverter), new PropertyMetadata(390D));

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