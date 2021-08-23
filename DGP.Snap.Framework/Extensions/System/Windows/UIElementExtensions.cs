using System.Windows;

namespace DGP.Snap.Framework.Extensions.System.Windows
{
    public static class UIElementExtensions
    {
        /// <summary>
        ///     显示元素
        /// </summary>
        /// <param name="element"></param>
        public static void Show(this UIElement element) => element.Visibility = Visibility.Visible;

        /// <summary>
        ///     显示元素
        /// </summary>
        /// <param name="element"></param>
        /// <param name="show"></param>
        public static void Show(this UIElement element, bool show) => element.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        ///     不现实元素，但保留空间
        /// </summary>
        /// <param name="element"></param>
        public static void Hide(this UIElement element) => element.Visibility = Visibility.Hidden;

        /// <summary>
        ///     不显示元素，且不保留空间
        /// </summary>
        /// <param name="element"></param>
        public static void Collapse(this UIElement element) => element.Visibility = Visibility.Collapsed;
    }
}
