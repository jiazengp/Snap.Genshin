using DGP.Genshin.Control;
using DGP.Genshin.Control.GenshinElement;
using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Helper;
using DGP.Genshin.MiHoYoAPI.Announcement;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Data.Primitive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class HomeViewModel : ObservableObject, ISupportCancellation
    {
        private readonly IHomeService homeService;
        public CancellationToken CancellationToken { get; set; }

        private AnnouncementWrapper? announcement;
        private string? manifesto;

        public AnnouncementWrapper? Announcement
        {
            get => announcement;

            set => SetProperty(ref announcement, value);
        }
        public string? Manifesto
        {
            get => manifesto;
            set => SetProperty(ref manifesto, value);
        }
        public WorkWatcher OpeningUI { get; } = new(false);
        public ICommand OpenUICommand { get; }
        public ICommand OpenAnnouncementUICommand { get; }

        public HomeViewModel(IHomeService homeService, IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            this.homeService = homeService;

            OpenUICommand = asyncRelayCommandFactory.Create(OpenUIAsync);
            OpenAnnouncementUICommand = new RelayCommand<string>(OpenAnnouncementUI);
        }

        private async Task OpenUIAsync()
        {
            using (OpeningUI.Watch())
            {
                try
                {
                    Manifesto = await homeService.GetManifestoAsync(CancellationToken);
                    Announcement = await homeService.GetAnnouncementsAsync(OpenAnnouncementUICommand, CancellationToken);
                }
                catch (TaskCanceledException) { this.Log("Open UI cancelled"); }
            }
        }
        private void OpenAnnouncementUI(string? content)
        {
            if (WebView2Helper.IsSupported)
            {
                using (AnnouncementWindow? window = new(content))
                {
                    window.ShowDialog();
                }
            }
            else
            {
                new WebView2RuntimeWindow().ShowDialog();
            }
        }
    }
}
