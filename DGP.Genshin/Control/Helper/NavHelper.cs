using ModernWpf.Controls;
using System;
using System.Windows;

namespace DGP.Genshin.Control.Helper
{
    public sealed class NavHelper
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


        public static object? GetExtraData(DependencyObject? obj)
        {
            return obj?.GetValue(ExtraDataProperty);
        }
        public static void SetExtraData(DependencyObject obj, object value)
        {
            obj.SetValue(ExtraDataProperty, value);
        }
        public static readonly DependencyProperty ExtraDataProperty =
            DependencyProperty.RegisterAttached("ExtraData", typeof(object), typeof(NavHelper), new PropertyMetadata(null));
    }
}
