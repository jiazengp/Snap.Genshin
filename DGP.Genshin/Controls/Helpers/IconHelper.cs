using System.Windows;

namespace DGP.Genshin.Controls.Helpers
{
    public class IconHelper 
    {
        public static string GetIcon(DependencyObject obj)
        {
            return (string)obj.GetValue(IconProperty);
        }

        public static void SetIcon(DependencyObject obj, string value)
        {
            obj.SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached("Icon", typeof(string), typeof(IconHelper), new PropertyMetadata(""));
    }
}
