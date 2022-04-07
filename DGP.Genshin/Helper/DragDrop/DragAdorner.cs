using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DGP.Genshin.Helper.DragDrop
{
    /// <summary>
    /// Renders a visual which can follow the mouse cursor,
    /// such as during a drag-and-drop operation.
    /// </summary>
    [SuppressMessage("", "SA1201")]
    [SuppressMessage("", "SA1614")]
    [SuppressMessage("", "SA1616")]
    public class DragAdorner : Adorner
    {
        private readonly Rectangle child;
        private double offsetLeft = 0;
        private double offsetTop = 0;

        /// <summary>
        /// Initializes a new instance of DragVisualAdorner.
        /// </summary>
        /// <param name="adornedElement">The element being adorned.</param>
        /// <param name="size">The size of the adorner.</param>
        /// <param name="brush">A brush to with which to paint the adorner.</param>
        public DragAdorner(UIElement adornedElement, Size size, Brush brush)
            : base(adornedElement)
        {
            Rectangle rect = new()
            {
                Fill = brush,
                Width = size.Width,
                Height = size.Height,
                IsHitTestVisible = false,
            };
            child = rect;
        }

        /// <summary>
        /// Override.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(offsetLeft, offsetTop));
            return result;
        }

        /// <summary>
        /// Gets/sets the horizontal offset of the adorner.
        /// </summary>
        public double OffsetLeft
        {
            get => offsetLeft;
            set
            {
                offsetLeft = value;
                UpdateLocation();
            }
        }

        /// <summary>
        /// Updates the location of the adorner in one atomic operation.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        public void SetOffsets(double left, double top)
        {
            offsetLeft = left;
            offsetTop = top;
            UpdateLocation();
        }

        /// <summary>
        /// Gets/sets the vertical offset of the adorner.
        /// </summary>
        public double OffsetTop
        {
            get => offsetTop;
            set
            {
                offsetTop = value;
                UpdateLocation();
            }
        }

        /// <summary>
        /// Override.
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {
            child.Measure(constraint);
            return child.DesiredSize;
        }

        /// <summary>
        /// Override.
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        /// <summary>
        /// Override.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override Visual GetVisualChild(int index)
        {
            return child;
        }

        /// <summary>
        /// Override.  Always returns 1.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get => 1;
        }

        private void UpdateLocation()
        {
            if (Parent is AdornerLayer adornerLayer)
            {
                adornerLayer.Update(AdornedElement);
            }
        }
    }
}