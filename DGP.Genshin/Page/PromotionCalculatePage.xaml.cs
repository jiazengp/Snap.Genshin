using DGP.Genshin.ViewModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DGP.Genshin.Page
{
    public partial class PromotionCalculatePage : System.Windows.Controls.Page
    {
        public PromotionCalculatePage()
        {
            DataContext = App.AutoWired<PromotionCalculateViewModel>();
            InitializeComponent();
        }

        #region NumberBoxDeleteButtonRemover
        private void NumberBoxValueChanged(ModernWpf.Controls.NumberBox sender, ModernWpf.Controls.NumberBoxValueChangedEventArgs args)
        {
            List<Button> buttons = new();
            FindChildren(sender, buttons);
            if (buttons.Count > 0)
            {
                ((Grid)buttons[0].Parent).Children.Remove(buttons[0]);
            }
        }

        private void NumberBoxGotFocus(object sender, RoutedEventArgs e)
        {
            List<Button> buttons = new();
            FindChildren((ModernWpf.Controls.NumberBox)sender, buttons);
            if (buttons.Count > 0)
            {
                ((Grid)buttons[0].Parent).Children.Remove(buttons[0]);
            }
        }

        internal static void FindChildren<T>(DependencyObject parent, List<T> results) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject current = VisualTreeHelper.GetChild(parent, i);
                if ((current.GetType()).Equals(typeof(T)) /*|| (current.GetType().GetTypeInfo().IsSubclassOf(typeof(T)))*/)
                {
                    T asType = (T)current;
                    results.Add(asType);
                }
                FindChildren<T>(current, results);
            }
        }
        #endregion
    }
}
