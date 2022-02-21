using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.Windows;

namespace DGP.Genshin.Control.Title
{
    public static class TitleBarButtonFlyoutHelper
    {
        /// <summary>
        /// 显示关联的 <see cref="Flyout"/> 控件
        /// </summary>
        /// <typeparam name="TContentType"></typeparam>
        /// <param name="button">标题栏按钮</param>
        /// <param name="dataContext">数据上下文</param>
        public static bool ShowAttachedFlyout<TContentType>(this TitleBarButton button, object dataContext) where TContentType : FrameworkElement
        {
            if (FlyoutBase.GetAttachedFlyout(button) is Flyout flyout)
            {
                if (flyout.Content is TContentType content)
                {
                    content.DataContext = dataContext;
                    FlyoutBase.ShowAttachedFlyout(button);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 关闭关联的 <see cref="Flyout"/> 控件
        /// </summary>
        /// <param name="button">待关闭的浮出控件</param>
        public static void HideAttachedFlyout(this TitleBarButton button)
        {
            if (FlyoutBase.GetAttachedFlyout(button) is Flyout flyout)
            {
                flyout.Hide();
            }
        }
    }
}
