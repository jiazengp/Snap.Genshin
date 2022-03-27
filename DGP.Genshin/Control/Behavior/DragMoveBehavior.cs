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
        /// <inheritdoc/>
        protected override void OnAttached()
        {
            this.AssociatedObject.MouseLeftButtonDown += this.MouseDown;
            base.OnAttached();
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            this.AssociatedObject.MouseLeftButtonDown -= this.MouseDown;
            base.OnDetaching();
        }

        private void MouseDown(object sender, EventArgs ea)
        {
            Window.GetWindow(sender as FrameworkElement)?.DragMove();
        }
    }
}