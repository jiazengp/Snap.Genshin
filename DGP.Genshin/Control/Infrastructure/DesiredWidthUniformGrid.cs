using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace DGP.Genshin.Control.Infrastructure
{
    /// <summary>
    /// 自适应宽度网格
    /// 会将栏数设为最接近设定栏宽的
    /// </summary>
    public sealed class DesiredWidthUniformGrid : UniformGrid
    {
        private static readonly DependencyProperty ColumnDesiredWidthProperty =
            Property<DesiredWidthUniformGrid>.Depend(nameof(ColumnDesiredWidth), 0D, OnColumnDesiredWidthChanged);

        /// <summary>
        /// 栏的期望宽度
        /// </summary>
        public double ColumnDesiredWidth
        {
            get => (double)this.GetValue(ColumnDesiredWidthProperty);

            set => this.SetValue(ColumnDesiredWidthProperty, value);
        }

        /// <inheritdoc/>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            this.SetCorrectColumn();
            base.OnRenderSizeChanged(sizeInfo);
        }

        private static void OnColumnDesiredWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DesiredWidthUniformGrid)d).SetCorrectColumn();
        }

        private void SetCorrectColumn()
        {
            if (this.ColumnDesiredWidth > 0)
            {
                this.Columns = (int)Math.Round(this.ActualWidth / this.ColumnDesiredWidth);
            }
        }
    }
}