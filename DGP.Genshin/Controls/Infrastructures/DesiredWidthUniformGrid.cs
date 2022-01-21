using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace DGP.Genshin.Controls.Infrastructures
{
    public class DesiredWidthUniformGrid : UniformGrid
    {
        public double ColumnDesiredWidth
        {
            get { return (double)GetValue(ColumnDesiredWidthProperty); }
            set { SetValue(ColumnDesiredWidthProperty, value); }
        }
        public static readonly DependencyProperty ColumnDesiredWidthProperty =
            DependencyProperty.Register(nameof(ColumnDesiredWidth), typeof(double), typeof(DesiredWidthUniformGrid),
                new PropertyMetadata(0D, OnColumnDesiredWidthChanged));

        private static void OnColumnDesiredWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DesiredWidthUniformGrid)d).SetCorrectColumn();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            SetCorrectColumn();
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void SetCorrectColumn()
        {
            Columns = (int)Math.Round(ActualWidth / ColumnDesiredWidth);
        }
    }
}
