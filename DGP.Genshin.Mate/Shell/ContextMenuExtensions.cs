using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Mate.Shell
{
    public static class ContextMenuExtensions
    {
        public static int AddItem(this ContextMenu menu, MenuItem item, RoutedEventHandler clickAction)
        {
            item.Click += clickAction;
            return menu.Items.Add(item);
        }
    }
}
