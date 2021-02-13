using DGP.Genshin.Helper;
using DGP.Genshin.Pages;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DGP.Genshin.Service
{
    public class NavigationService
    {
        private readonly Frame frame;
        private readonly NavigationView navigationView;
        private readonly Stack<NavigationViewItem> backItemStack = new Stack<NavigationViewItem>();

        private NavigationViewItem selected;

        public EventHandler<BackRequestedEventArgs> BackRequestedEventHandler;

        public NavigationService(Window window, NavigationView navigationView, Frame frame)
        {
            this.navigationView = navigationView;
            this.frame = frame;

            this.navigationView.ItemInvoked += this.OnItemInvoked;
            this.BackRequestedEventHandler += this.OnBackRequested;
            TitleBar.AddBackRequestedHandler(window, this.BackRequestedEventHandler);
        }

        public void SyncTabWith(Type pageType)
        {
            if (pageType == typeof(SettingsPage) || pageType == null)
            {
                this.navigationView.SelectedItem = this.navigationView.SettingsItem;
            }
            else
            {
                NavigationViewItem target = this.navigationView.MenuItems.OfType<NavigationViewItem>()
                    .First(menuItem => ((Type)menuItem.GetValue(NavHelper.NavigateToProperty)) == pageType);
                this.navigationView.SelectedItem = target;
            }
            this.selected = this.navigationView.SelectedItem as NavigationViewItem;
        }

        public bool Navigate(Type pageType, bool isSyncTabRequested = false, object data = null, NavigationTransitionInfo info = null)
        {
            if (isSyncTabRequested)
            {
                this.SyncTabWith(pageType);
            }

            this.backItemStack.Push(this.selected);
            Debug.WriteLine(this.backItemStack.Count);
            return this.frame.Navigate(pageType, data, info);
        }
        public bool Navigate<T>(bool isSyncTabRequested = false, object data = null, NavigationTransitionInfo info = null) where T : System.Windows.Controls.Page => this.Navigate(typeof(T), isSyncTabRequested, data, info);

        public bool CanGoBack => this.frame.CanGoBack;
        private void GoBack() => this.frame.GoBack();

        private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            this.selected = this.navigationView.SelectedItem as NavigationViewItem;
            if (args.IsSettingsInvoked)
            {
                this.Navigate<SettingsPage>();
            }
            else
            {
                this.Navigate(this.selected.GetValue(NavHelper.NavigateToProperty) as Type);
            }
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs args)
        {
            if (this.CanGoBack)
            {
                this.backItemStack.Pop();
                NavigationViewItem back = this.backItemStack.Peek();
                this.SyncTabWith(back.GetValue(NavHelper.NavigateToProperty) as Type);
                this.GoBack();
            }
        }
    }
}
