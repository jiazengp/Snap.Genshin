using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Message.Internal;
using DGP.Genshin.Service.Abstraction;
using DGP.Genshin.Service.Abstraction.IntegrityCheck;
using Microsoft.Toolkit.Mvvm.Messaging;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using Snap.Data.Primitive;
using Snap.Net.Networking;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    /// <summary>
    /// 启动界面视图模型
    /// </summary>
    [ViewModel(InjectAs.Transient)]
    internal class SplashViewModel : ObservableObject2
    {
        private readonly ICookieService cookieService;
        private readonly IIntegrityCheckService integrityCheckService;
        private readonly IMessenger messenger;

        #region Observable
        private bool isCookieVisible = false;
        private bool isSplashNotVisible = false;
        private string currentStateDescription = "初始化...";
        private int currentCount;
        private string? currentInfo;
        private int? totalCount;
        private double percent;

        public bool IsCookieVisible
        {
            get => this.isCookieVisible;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.isCookieVisible, value, this.TrySendCompletedMessage);
        }

        [PropertyChangedCallback]
        private void TrySendCompletedMessage()
        {
            if (this.IsCookieVisible == false && this.integrityCheckService.IntegrityChecking.IsCompleted)
            {
                this.messenger.Send(new SplashInitializationCompletedMessage(this));
            }
        }
        /// <summary>
        /// 设置为 <see cref="true"/> 以触发淡入主界面动画
        /// </summary>
        public bool IsSplashNotVisible
        {
            get => this.isSplashNotVisible;

            set => this.SetProperty(ref this.isSplashNotVisible, value);
        }
        public string CurrentStateDescription
        {
            get => this.currentStateDescription;

            set => this.SetProperty(ref this.currentStateDescription, value);
        }
        public int CurrentCount
        {
            get => this.currentCount;

            set => this.SetProperty(ref this.currentCount, value);
        }
        public string? CurrentInfo
        {
            get => this.currentInfo;

            set => this.SetProperty(ref this.currentInfo, value);
        }
        public int? TotalCount
        {
            get => this.totalCount;

            set => this.SetProperty(ref this.totalCount, value);
        }
        public double Percent
        {
            get => this.percent;

            set => this.SetProperty(ref this.percent, value);
        }
        public WorkWatcher IntegrityChecking { get; set; } = new();
        #endregion

        public ICommand OpenUICommand { get; }
        public ICommand SetCookieCommand { get; }

        /// <summary>
        /// 通知 <see cref="SplashViewModel"/> 结束初始化
        /// </summary>
        public void CompleteInitialization()
        {
            this.CurrentStateDescription = "完成";
            this.IsSplashNotVisible = true;
        }

        public SplashViewModel(ICookieService cookieService, IIntegrityCheckService integrityCheckService, IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger)
        {
            this.cookieService = cookieService;
            this.integrityCheckService = integrityCheckService;
            this.messenger = messenger;

            this.SetCookieCommand = asyncRelayCommandFactory.Create(this.SetCookieAsync);
            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
        }

        private async Task SetCookieAsync()
        {
            await this.cookieService.SetCookieAsync();
            this.IsCookieVisible = !this.cookieService.IsCookieAvailable;
        }

        private async Task OpenUIAsync()
        {
            this.CurrentStateDescription = "等待网络连接...";
            await Network.WaitConnectionAsync();
            this.CurrentStateDescription = "校验 Cookie 有效性...";
            await this.PerformCookieServiceCheckAsync();
            this.CurrentStateDescription = "校验 缓存资源 完整性...";
            await this.PerformIntegrityServiceCheckAsync();
            this.CurrentStateDescription = string.Empty;
            this.TrySendCompletedMessage();
        }

        private async Task PerformCookieServiceCheckAsync()
        {
            await this.cookieService.InitializeAsync();
            this.IsCookieVisible = !this.cookieService.IsCookieAvailable;
        }

        private async Task PerformIntegrityServiceCheckAsync()
        {
            Progress<IIntegrityCheckService.IIntegrityCheckState> progress = new();
            progress.ProgressChanged += (_, state) =>
            {
                this.CurrentCount = state.CurrentCount;
                this.Percent = (state.CurrentCount * 1D / this.TotalCount) ?? 0D;
                this.TotalCount = state.TotalCount;
                this.CurrentInfo = state.Info;
            };

            using (this.IntegrityChecking.Watch())
            {
                await this.integrityCheckService.CheckMetadataIntegrityAsync(progress);
            }
        }
    }
}
