using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Helpers;
using DGP.Genshin.Pages;
using DGP.Genshin.Services.Abstratcions;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System;
using System.Linq;

namespace DGP.Genshin.Services
{

    /// <summary>
    /// 导航服务
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
                if(navigationView != null)
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
        public void SyncTabWith(Type pageType)
        {
            if(NavigationView is null)
            {
                return;
            }

            if (pageType == typeof(SettingsPage))
            {
                NavigationView.SelectedItem = NavigationView.SettingsItem;
            }
            else
            {
                NavigationViewItem? target = NavigationView.MenuItems.OfType<NavigationViewItem>()
                .First(menuItem => ((Type)menuItem.GetValue(NavHelper.NavigateToProperty)) == pageType);
                NavigationView.SelectedItem = target;
            }

            Selected = NavigationView.SelectedItem as NavigationViewItem;
        }
        public bool Navigate(Type? pageType, bool isSyncTabRequested = false, object? data = null, NavigationTransitionInfo? info = null)
        {
            if (pageType is null)
            {
                return false;
            }

            HasEverNavigated = true;
            if (Frame?.Content?.GetType() == pageType)
            {
                return false;
            }
            if (isSyncTabRequested)
            {
                SyncTabWith(pageType);
            }
            //bool result = this.frame.Navigate(pageType, data, info);
            bool result = Frame?.Navigate(pageType, data, new DrillInNavigationTransitionInfo()) ?? false;
            this.Log($"navigate to {pageType}:{(result ? "succeed" : "failed")}");
            //fix memory leak issue
            Frame?.RemoveBackEntry();
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
