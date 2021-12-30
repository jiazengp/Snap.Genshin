using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;

namespace DGP.Genshin.Controls.Behaviors
{
    public class DragMoveBehavior : Behavior<FrameworkElement>
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
        void MouseDown(object sender, EventArgs ea)
        {
            Window.GetWindow(sender as FrameworkElement)?.DragMove();
        }
    }
}
