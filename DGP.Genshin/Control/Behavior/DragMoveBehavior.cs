using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;

namespace DGP.Genshin.Control.Behavior
{
    /// <summary>
    /// 允许控件实现 <see cref="Window.DragMove"/>
    /// </summary>
    public sealed class DragMoveBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.MouseLeftButtonDown += MouseDown;
            base.OnAttached();
        }
        protected override void OnDetaching()
        {
            AssociatedObject.MouseLeftButtonDown -= MouseDown;
            base.OnDetaching();
        }

        private void MouseDown(object sender, EventArgs ea)
        {
            Window.GetWindow(sender as FrameworkElement)?.DragMove();
        }
    }
}
