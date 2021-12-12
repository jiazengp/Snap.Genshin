using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface INavigationService
    {
        bool HasEverNavigated { get; set; }
        Frame? Frame { get; set; }
        NavigationView? NavigationView { get; set; }
        NavigationViewItem? Selected { get; set; }

        bool Navigate(Type? pageType, bool isSyncTabRequested = false, object? data = null, NavigationTransitionInfo? info = null);
        bool Navigate<T>(bool isSyncTabRequested = false, object? data = null, NavigationTransitionInfo? info = null) where T : System.Windows.Controls.Page;
        void SyncTabWith(Type pageType);
    }
}
