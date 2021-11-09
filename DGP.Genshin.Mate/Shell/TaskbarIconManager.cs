using DGP.Genshin.Mate.Views;
using Hardcodet.Wpf.TaskbarNotification;
using ModernWpf.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DGP.Genshin.Mate.Shell
{
    public class TaskbarIconManager : IDisposable
    {
        private readonly TaskbarIcon taskbarIcon;

        /// <summary>
        /// 查找 <see cref="Application.Current.Windows"/> 集合中的对应 <typeparamref name="TWindow"/> 类型的 Window
        /// </summary>
        /// <returns>返回唯一的窗口，未找到返回新实例</returns>
        public static Window GetOrAddWindow<TWindow>() where TWindow : Window, new()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(TWindow))
                {
                    return window;
                }
            }
            return new TWindow();
        }

        public void Dispose()
        {
            ((IDisposable)taskbarIcon).Dispose();
        }

        #region 单例
        private static volatile TaskbarIconManager? instance;
        [SuppressMessage("", "IDE0044")]
        private static object _locker = new();
        private TaskbarIconManager()
        {
            taskbarIcon = new TaskbarIcon
            {
                MenuActivation = PopupActivationMode.All,
                IconSource = new BitmapImage(new Uri("pack://application:,,,/DGP.Genshin.Mate;component/SGM_Logo.ico"))
            };
            TaskbarIconContextMenu menu = new();
            var autorunMenu = new MenuItem { Header = "开机启动" };
            autorunMenu.Icon = AutoRunHelper.IsAutoRun ? new FontIcon { Glyph = "\xE73E" } : null;
            autorunMenu.Click += (s, e) => 
            {
                AutoRunHelper.IsAutoRun = !AutoRunHelper.IsAutoRun;
                autorunMenu.Icon = AutoRunHelper.IsAutoRun ? new FontIcon { Glyph = "\xE73E" } : null;
            };

            menu.Items.Add(autorunMenu);
            menu.AddItem(new MenuItem { Header = "退出", Icon = new FontIcon { Glyph = "\xE7E8" } }, (s, e) => App.Current.Shutdown());
            taskbarIcon.ContextMenu = menu;
        }

        public static TaskbarIconManager Instance
        {
            get
            {
                if (instance is null)
                {
                    lock (_locker)
                    {
                        instance ??= new();
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
