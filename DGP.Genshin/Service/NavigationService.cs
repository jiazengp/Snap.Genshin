using DGP.Genshin.Control.Helper;
using DGP.Genshin.Core.Plugins;
using DGP.Genshin.DataModel.WebViewLobby;
using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstraction;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using System;
using System.Collections.ObjectModel;
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
            Type? currntType = Frame?.Content?.GetType();
            if (pageType is null || (currntType == pageType && currntType != typeof(WebViewHostPage)))
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

        public bool Navigate(NavigateRequestMessage message)
        {
            return Navigate(message.Value, message.IsSyncTabRequested, message.ExtraData);
        }

        public bool Navigate<T>(bool isSyncTabRequested = false, object? data = null, NavigationTransitionInfo? info = null)
            where T : System.Windows.Controls.Page
        {
            return Navigate(typeof(T), isSyncTabRequested, data, info);
        }

        private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            Selected = NavigationView?.SelectedItem as NavigationViewItem;
            Type? targetType = args.IsSettingsInvoked ? typeof(SettingsPage) : NavHelper.GetNavigateTo(Selected);
            Navigate(targetType, false, NavHelper.GetExtraData(Selected));
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

        public void AddWebViewEntries(ObservableCollection<WebViewEntry>? entries)
        {
            if (NavigationView is null)
            {
                return;
            }

            if (entries is not null)
            {
                NavigationViewItemHeader? header = NavigationView.MenuItems
                    .OfType<NavigationViewItemHeader>()
                    .SingleOrDefault(header => header.Content.ToString() == "网页");
                if (header is not null)
                {
                    int headerIndex = NavigationView.MenuItems.IndexOf(header);

                    if (entries.Count > 0)
                    {
                        header.Visibility = System.Windows.Visibility.Visible;
                    }

                    foreach (WebViewEntry entry in entries)
                    {
                        if (entry.ShowInNavView)
                        {
                            BitmapIcon icon = new() { ShowAsMonochrome = false };

                            icon.UriSource = Uri.TryCreate(entry.IconUrl, UriKind.Absolute, out Uri? iconUri)
                                ? iconUri
                                : new Uri("pack://application:,,,/SG_Logo.ico");

                            NavigationViewItem item = new()
                            {
                                Icon = icon,
                                Content = entry.Name,
                            };

                            NavHelper.SetNavigateTo(item, typeof(WebViewHostPage));
                            NavHelper.SetExtraData(item, entry);

                            NavigationView.MenuItems.Insert(++headerIndex, item);
                        }
                    }
                }
            }
        }
    }
}
