using DGP.Genshin.Helper;
using Microsoft;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DGP.Genshin.Control.Converter
{
    /// <summary>
    /// 百分比转换高度
    /// </summary>
    public sealed class PercentageToHeightConverter : DependencyObject, IValueConverter
    {
        private static readonly DependencyProperty TargetWidthProperty = Property<PercentageToHeightConverter>.Depend(nameof(TargetWidth), 1080D);
        private static readonly DependencyProperty TargetHeightProperty = Property<PercentageToHeightConverter>.Depend(nameof(TargetHeight), 390D);

        /// <summary>
        /// 目标宽度
        /// </summary>
        public double TargetWidth
        {
            get => (double)this.GetValue(TargetWidthProperty);

            set => this.SetValue(TargetWidthProperty, value);
        }

        /// <summary>
        /// 目标高度
        /// </summary>
        public double TargetHeight
        {
            get => (double)this.GetValue(TargetHeightProperty);

            set => this.SetValue(TargetHeightProperty, value);
        }

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value * (this.TargetHeight / this.TargetWidth);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw Assumes.NotReachable();
        }
    }
}