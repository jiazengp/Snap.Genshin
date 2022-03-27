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
            get => this.announcement;

            set => this.SetProperty(ref this.announcement, value);
        }
        public string? Manifesto
        {
            get => this.manifesto;
            set => this.SetProperty(ref this.manifesto, value);
        }
        public WorkWatcher OpeningUI { get; } = new(false);
        public ICommand OpenUICommand { get; }
        public ICommand OpenAnnouncementUICommand { get; }

        public HomeViewModel(IHomeService homeService, IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            this.homeService = homeService;

            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
            this.OpenAnnouncementUICommand = new RelayCommand<string>(this.OpenAnnouncementUI);
        }

        private async Task OpenUIAsync()
        {
            using (this.OpeningUI.Watch())
            {
                try
                {
                    this.Manifesto = await this.homeService.GetManifestoAsync(this.CancellationToken);
                    this.Announcement = await this.homeService.GetAnnouncementsAsync(this.OpenAnnouncementUICommand, this.CancellationToken);
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
