using DGP.Genshin.Helper;
using Microsoft;
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
        private static readonly DependencyProperty TrueStyleProperty = Property<BooleanToStyleConverter>.Depend<Style>(nameof(TrueStyle));
        private static readonly DependencyProperty FalseStyleProperty = Property<BooleanToStyleConverter>.Depend<Style>(nameof(FalseStyle));

        /// <summary>
        /// 当值为<see cref="true"/>时的样式
        /// </summary>
        public Style TrueStyle
        {
            get => (Style)this.GetValue(TrueStyleProperty);

            set => this.SetValue(TrueStyleProperty, value);
        }

        /// <summary>
        /// 当值为<see cref="true"/>时的样式
        /// </summary>
        public Style FalseStyle
        {
            get => (Style)this.GetValue(FalseStyleProperty);

            set => this.SetValue(FalseStyleProperty, value);
        }

        /// <inheritdoc/>
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

            return !flag ? this.FalseStyle : this.TrueStyle;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw Assumes.NotReachable();
        }
    }
}