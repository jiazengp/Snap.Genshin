using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DGP.Genshin.Control;
using DGP.Genshin.Control.GenshinElement;
using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.MiHoYoAPI.Announcement;
using DGP.Genshin.Service.Abstraction;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Data.Primitive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    /// <summary>
    /// 主页视图模型
    /// </summary>
    [ViewModel(InjectAs.Transient)]
    internal class HomeViewModel : ObservableObject, ISupportCancellation
    {
        private readonly IHomeService homeService;

        private AnnouncementWrapper? announcement;
        private string? manifesto;

        /// <summary>
        /// 构造一个主页视图模型
        /// </summary>
        /// <param name="homeService">主页服务</param>
        /// <param name="asyncRelayCommandFactory">异步命令工厂</param>
        public HomeViewModel(IHomeService homeService, IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            this.homeService = homeService;

            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
            this.OpenAnnouncementUICommand = new RelayCommand<string>(this.OpenAnnouncementUI);
        }

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// 公告
        /// </summary>
        public AnnouncementWrapper? Announcement
        {
            get => this.announcement;

            set => this.SetProperty(ref this.announcement, value);
        }

        /// <summary>
        /// Snap Genshin 公告
        /// </summary>
        public string? Manifesto
        {
            get => this.manifesto;
            set => this.SetProperty(ref this.manifesto, value);
        }

        /// <summary>
        /// 打开界面监视器
        /// </summary>
        public WorkWatcher OpeningUI { get; } = new(false);

        /// <summary>
        /// 打开界面触发的命令
        /// </summary>
        public ICommand OpenUICommand { get; }

        /// <summary>
        /// 打开公告UI触发的命令
        /// </summary>
        public ICommand OpenAnnouncementUICommand { get; }

        private async Task OpenUIAsync()
        {
            using (this.OpeningUI.Watch())
            {
                try
                {
                    this.Manifesto = await this.homeService.GetManifestoAsync(this.CancellationToken);
                    this.Announcement = await this.homeService.GetAnnouncementsAsync(this.OpenAnnouncementUICommand, this.CancellationToken);
                }
                catch (TaskCanceledException)
                {
                    this.Log("Open UI cancelled");
                }
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