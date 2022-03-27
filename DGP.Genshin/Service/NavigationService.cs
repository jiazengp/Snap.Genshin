using DGP.Genshin.Control.Helper;
using DGP.Genshin.Core.Plugins;
using DGP.Genshin.DataModel.WebViewLobby;
using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Messaging;
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
    internal class NavigationService : INavigationService, IRecipient<NavigateRequestMessage>
    {
        private readonly IMessenger messenger;
        private NavigationView? navigationView;

        public Frame? Frame { get; set; }

        public NavigationView? NavigationView
        {
            get => this.navigationView;

            set
            {
                //remove old listener
                if (this.navigationView != null)
                {
                    this.navigationView.ItemInvoked -= this.OnItemInvoked;
                }
                this.navigationView = value;
                //add new listener
                if (this.navigationView != null)
                {
                    this.navigationView.ItemInvoked += this.OnItemInvoked;
                }
            }
        }
        public NavigationViewItem? Selected { get; set; }
        public bool HasEverNavigated { get; set; }

        public NavigationService(IMessenger messenger)
        {
            this.messenger = messenger;
            messenger.RegisterAll(this);
        }

        ~NavigationService()
        {
            this.messenger.UnregisterAll(this);
        }

        public bool SyncTabWith(Type pageType)
        {
            if (this.NavigationView is null)
            {
                return false;
            }

            if (pageType == typeof(SettingPage))
            {
                this.NavigationView.SelectedItem = this.NavigationView.SettingsItem;
            }
            else
            {
                NavigationViewItem? target = this.NavigationView.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(menuItem => NavHelper.GetNavigateTo(menuItem) == pageType);
                this.NavigationView.SelectedItem = target;
            }
            this.Selected = this.NavigationView.SelectedItem as NavigationViewItem;
            return true;
        }

        public bool Navigate(Type? pageType, bool isSyncTabRequested = false, object? data = null, NavigationTransitionInfo? info = null)
        {
            Type? currentType = this.Frame?.Content?.GetType();
            if (pageType is null || (currentType == pageType && currentType != typeof(WebViewHostPage)))
            {
                return false;
            }
            _ = isSyncTabRequested && this.SyncTabWith(pageType);

            bool result = false;
            try
            {
                result = this.Frame?.Navigate(App.AutoWired(pageType), data) ?? false;
            }
            catch { }
            this.Log($"Navigate to {pageType}:{(result ? "succeed" : "failed")}");
            //分析页面统计数据时不应加入启动时导航的首个页面
            if (this.HasEverNavigated)
            {
                new Event(pageType, result).TrackAs(Event.OpenUI);
            }
            //fix memory leak issue
            this.Frame?.RemoveBackEntry();
            //首次导航失败时使属性持续保存为false
            this.HasEverNavigated |= result;
            return result;
        }

        public bool Navigate(NavigateRequestMessage message)
        {
            return this.Navigate(message.Value, message.IsSyncTabRequested, message.ExtraData);
        }

        public bool Navigate<T>(bool isSyncTabRequested = false, object? data = null, NavigationTransitionInfo? info = null)
            where T : System.Windows.Controls.Page
        {
            return this.Navigate(typeof(T), isSyncTabRequested, data, info);
        }

        private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            this.Selected = this.NavigationView?.SelectedItem as NavigationViewItem;
            Type? targetType = args.IsSettingsInvoked ? typeof(SettingPage) : NavHelper.GetNavigateTo(this.Selected);
            this.Navigate(targetType, false, NavHelper.GetExtraData(this.Selected));
        }

        public bool AddToNavigation(ImportPageAttribute importPage)
        {
            return this.AddToNavigation(importPage.PageType, importPage.Label, importPage.Icon);
        }

        private bool AddToNavigation(Type pageType, string label, IconElement icon)
        {
            if (this.NavigationView is null)
            {
                return false;
            }
            NavigationViewItem item = new() { Content = label, Icon = icon };
            NavHelper.SetNavigateTo(item, pageType);
            this.Log($"Add {pageType} to NavigationView");
            return this.NavigationView.MenuItems.Add(item) != -1;
        }

        public void AddWebViewEntries(ObservableCollection<WebViewEntry>? entries)
        {
            if (this.NavigationView is null)
            {
                return;
            }

            if (entries is not null)
            {
                NavigationViewItemHeader? header = this.NavigationView.MenuItems
                    .OfType<NavigationViewItemHeader>()
                    .SingleOrDefault(header => header.Content.ToString() == "网页");
                if (header is not null)
                {
                    int headerIndex = this.NavigationView.MenuItems.IndexOf(header);

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

                            this.NavigationView.MenuItems.Insert(++headerIndex, item);
                        }
                    }
                }
            }
        }

        public void Receive(NavigateRequestMessage message)
        {
            this.Navigate(message);
        }
    }
}
