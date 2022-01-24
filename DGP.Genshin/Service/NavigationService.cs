using DGP.Genshin.Control.Helper;
using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Helper;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstratcion;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using System;
using System.Linq;

namespace DGP.Genshin.Service
{
    /// <summary>
    /// 导航服务的默认实现
    /// </summary>
    [Service(typeof(INavigationService), InjectAs.Transient)]
    internal class NavigationService : INavigationService
    {
        private NavigationView? navigationView;

        public Frame? Frame { get; set; }
        public NavigationView? NavigationView
        {
            get => navigationView; set
            {
                //remove old listener
                if (navigationView != null)
                {
                    navigationView.ItemInvoked -= OnItemInvoked;
                }
                navigationView = value;
                //add new listener
                if (navigationView != null)
                {
                    navigationView.ItemInvoked += OnItemInvoked;
                }
            }
        }
        public NavigationViewItem? Selected { get; set; }
        public bool HasEverNavigated { get; set; }

        public bool SyncTabWith(Type pageType)
        {
            if (NavigationView is null)
            {
                return false;
            }

            if (pageType == typeof(SettingsPage))
            {
                NavigationView.SelectedItem = NavigationView.SettingsItem;
            }
            else
            {
                NavigationViewItem? target = NavigationView.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(menuItem => NavHelper.GetNavigateTo(menuItem) == pageType);
                NavigationView.SelectedItem = target;
            }
            Selected = NavigationView.SelectedItem as NavigationViewItem;
            return true;
        }

        public bool Navigate(Type? pageType, bool isSyncTabRequested = false, object? data = null, NavigationTransitionInfo? info = null)
        {
            if (pageType is null || Frame?.Content?.GetType() == pageType)
            {
                return false;
            }
            _ = isSyncTabRequested && SyncTabWith(pageType);
            bool result = false;
            try
            {
                result = Frame?.Navigate(pageType, data, new DrillInNavigationTransitionInfo()) ?? false;
            }
            catch { }
            this.Log($"Navigate to {pageType}:{(result ? "succeed" : "failed")}");
            //分析页面统计数据时不应加入启动时导航的首个页面
            if (HasEverNavigated)
            {
                new Event(pageType, result).TrackAs(Event.OpenUI);
            }
            //fix memory leak issue
            Frame?.RemoveBackEntry();
            //导航失败时使属性保存为false
            HasEverNavigated |= result;
            return result;
        }

        public bool Navigate<T>(bool isSyncTabRequested = false, object? data = null, NavigationTransitionInfo? info = null)
            where T : System.Windows.Controls.Page
        {
            return Navigate(typeof(T), isSyncTabRequested, data, info);
        }

        private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            Selected = NavigationView?.SelectedItem as NavigationViewItem;
            if (args.IsSettingsInvoked)
            {
                Navigate<SettingsPage>();
            }
            else
            {
                Navigate(NavHelper.GetNavigateTo(Selected));
            }
        }

        public bool AddToNavigation(ImportPageAttribute importPage)
        {
            return AddToNavigation(importPage.PageType, importPage.Label, importPage.Icon);
        }

        private bool AddToNavigation(Type pageType, string label, IconElement icon)
        {
            if (NavigationView is null)
            {
                return false;
            }
            NavigationViewItem item = new() { Content = label, Icon = icon };
            NavHelper.SetNavigateTo(item, pageType);
            this.Log($"Add {pageType} to NavigationView");
            return NavigationView.MenuItems.Add(item) != -1;
        }
    }
}
