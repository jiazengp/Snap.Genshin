using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DGP.Snap.Framework.Extensions.System.Windows
{
    public static class DependencyObjectExtensions
    {
        public static IEnumerable<T> VisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                VisualTreeHelper.GetChildrenCount(depObj).DebugWriteLine();
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }

                    foreach (T childOfChild in VisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
