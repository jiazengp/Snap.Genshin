using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.Windows;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    public static class TitleBarButtonFlyoutHelper
    {
        /// <summary>
        /// 显示关联的 <see cref="Flyout"/> 控件
        /// </summary>
        /// <typeparam name="TContentType"></typeparam>
        /// <param name="button">我们假定你传入了正确的类型</param>
        /// <param name="dataContext"></param>
        public static bool ShowAttachedFlyout<TContentType>(this object button, object dataContext) where TContentType : FrameworkElement
        {
            if (FlyoutBase.GetAttachedFlyout((TitleBarButton)button) is Flyout flyout)
            {
                if (flyout.Content is TContentType grid)
                {
                    grid.DataContext = dataContext;
                    FlyoutBase.ShowAttachedFlyout((TitleBarButton)button);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 关闭关联的 <see cref="Flyout"/> 控件
        /// </summary>
        /// <typeparam name="TContentType"></typeparam>
        /// <param name="button">我们假定你传入了正确的类型</param>
        /// <param name="dataContext"></param>
        public static void HideAttachedFlyout(this TitleBarButton button)
        {
            if (FlyoutBase.GetAttachedFlyout(button) is Flyout flyout)
            {
                flyout.Hide();
            }
        }
    }
}
