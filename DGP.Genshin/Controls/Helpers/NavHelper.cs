using ModernWpf.Controls;
using System;
using System.Windows;

namespace DGP.Genshin.Controls.Helpers
{
    public class NavHelper
    {
        public static Type? GetNavigateTo(NavigationViewItem? item)
        {
            return item?.GetValue(NavigateToProperty) as Type;
        }

        public static void SetNavigateTo(NavigationViewItem item, Type value)
        {
            item.SetValue(NavigateToProperty, value);
        }

        public static readonly DependencyProperty NavigateToProperty =
            DependencyProperty.RegisterAttached("NavigateTo", typeof(Type), typeof(NavHelper), new PropertyMetadata(null));
    }
}
