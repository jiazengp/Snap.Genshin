using DGP.Genshin.Control;
using DGP.Genshin.Control.GenshinElement;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Helper;
using DGP.Genshin.MiHoYoAPI.Announcement;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using Snap.Data.Primitive;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    public class HomeViewModel : ObservableObject
    {
        private readonly IHomeService homeService;

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
                Manifesto = await homeService.GetManifestoAsync();
                Announcement = await homeService.GetAnnouncementsAsync(OpenAnnouncementUICommand);
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
