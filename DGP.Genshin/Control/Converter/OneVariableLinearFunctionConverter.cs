using DGP.Genshin.Helper;
using Microsoft;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DGP.Genshin.Control.Converter
{
    /// <summary>
    /// 一元线性函数转换器
    /// Y = A * X + B
    /// </summary>
    public sealed class OneVariableLinearFunctionConverter : DependencyObject, IValueConverter
    {
        private static readonly DependencyProperty AProperty = Property<OneVariableLinearFunctionConverter>.Depend(nameof(A), 0D);
        private static readonly DependencyProperty BProperty = Property<OneVariableLinearFunctionConverter>.Depend(nameof(B), 0D);

        /// <summary>
        /// A
        /// </summary>
        public double A
        {
            get => (double)this.GetValue(AProperty);
            set => this.SetValue(AProperty, value);
        }

        /// <summary>
        /// B
        /// </summary>
        public double B
        {
            get => (double)this.GetValue(BProperty);
            set => this.SetValue(BProperty, value);
        }

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (this.A * (double)value) + this.B;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw Assumes.NotReachable();
        }
    }
}