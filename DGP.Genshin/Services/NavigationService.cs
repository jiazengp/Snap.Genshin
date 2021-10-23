using DGP.Genshin.Helpers;
using DGP.Genshin.Pages;
using DGP.Snap.Framework.Extensions.System;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// 导航服务
    /// </summary>
    public class NavigationService
    {
        private readonly Frame frame;
        private readonly NavigationView navigationView;
        private readonly Stack<NavigationViewItem> backItemStack = new Stack<NavigationViewItem>();

        private NavigationViewItem? selected;

        public NavigationService(Window window, NavigationView navigationView, Frame frame)
        {
            Current = Current == null ? this : throw new InvalidOperationException($"{nameof(NavigationService)}的实例在运行期间仅允许创建一次");
            this.navigationView = navigationView;
            this.frame = frame;

            this.navigationView.ItemInvoked += OnItemInvoked;
        }


        public static NavigationService? Current;
        public bool HasEverNavigated { get; set; } = false;

        public void SyncTabWith(Type pageType)
        {
            if (pageType == typeof(SettingsPage) || pageType == null)
            {
                navigationView.SelectedItem = navigationView.SettingsItem;
            }
            else
            {
                NavigationViewItem target = navigationView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(menuItem => ((Type)menuItem.GetValue(NavHelper.NavigateToProperty)) == pageType);
                navigationView.SelectedItem = target;
            }

            selected = navigationView.SelectedItem as NavigationViewItem;
        }

        public bool Navigate(Type? pageType, bool isSyncTabRequested = false, object? data = null, NavigationTransitionInfo? info = null)
        {
            if (pageType is null)
            {
                return false;
            }

            HasEverNavigated = true;
            if (frame.Content?.GetType() == pageType)
            {
                return false;
            }
            if (isSyncTabRequested)
            {
                SyncTabWith(pageType);
            }
            if (selected is not null)
            {
                backItemStack.Push(selected);
            }
            //bool result = this.frame.Navigate(pageType, data, info);
            bool result = frame.Navigate(pageType, data, new DrillInNavigationTransitionInfo());
            this.Log($"navigate to {pageType}:{result}");
            //fix memory leak issue
            frame.RemoveBackEntry();
            return result;
        }
        public bool Navigate<T>(bool isSyncTabRequested = false, object? data = null, NavigationTransitionInfo? info = null)
            where T : System.Windows.Controls.Page
        {
            return Navigate(typeof(T), isSyncTabRequested, data, info);
        }

        private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            selected = navigationView.SelectedItem as NavigationViewItem;
            if (args.IsSettingsInvoked)
            {
                Navigate<SettingsPage>(false);
            }
            else
            {
                Navigate(selected?.GetValue(NavHelper.NavigateToProperty) as Type);
            }
        }
    }
}
