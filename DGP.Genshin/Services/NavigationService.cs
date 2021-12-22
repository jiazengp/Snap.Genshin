using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Helpers;
using DGP.Genshin.Pages;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.AppCenter.Analytics;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System;
using System.Linq;

namespace DGP.Genshin.Services
{

    /// <summary>
    /// 导航服务的默认实现
    /// </summary>
    [Service(typeof(INavigationService), ServiceType.Singleton)]
    public class NavigationService : INavigationService
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

        public NavigationService() { }

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
                    .FirstOrDefault(menuItem => ((Type)menuItem.GetValue(NavHelper.NavigateToProperty)) == pageType);
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
            bool result = Frame?.Navigate(pageType, data, new DrillInNavigationTransitionInfo()) ?? false;
            this.Log($"Navigate to {pageType}:{(result ? "succeed" : "failed")}");
            //分析页面统计数据时不应加入启动时导航的首个页面
            if (HasEverNavigated&&result)
            {
                Analytics.TrackEvent("OpenUI", new Info(pageType, result).Build());
            }
            //fix memory leak? issue
            Frame?.RemoveBackEntry();
            HasEverNavigated = result;
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
                Navigate(Selected?.GetValue(NavHelper.NavigateToProperty) as Type);
            }
        }
    }
}
