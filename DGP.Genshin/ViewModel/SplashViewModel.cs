using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Message;
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
    public class SplashViewModel : ObservableObject2
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
            get => isCookieVisible;

            set => SetPropertyAndCallbackOnCompletion(ref isCookieVisible, value, TrySendCompletedMessage);
        }

        [PropertyChangedCallback]
        private void TrySendCompletedMessage()
        {
            if (IsCookieVisible == false && integrityCheckService.IntegrityChecking.IsCompleted)
            {
                messenger.Send(new SplashInitializationCompletedMessage(this));
            }
        }
        /// <summary>
        /// 设置为 <see cref="true"/> 以触发淡入主界面动画
        /// </summary>
        public bool IsSplashNotVisible
        {
            get => isSplashNotVisible;

            set => SetProperty(ref isSplashNotVisible, value);
        }
        public string CurrentStateDescription
        {
            get => currentStateDescription;

            set => SetProperty(ref currentStateDescription, value);
        }
        public int CurrentCount
        {
            get => currentCount;

            set => SetProperty(ref currentCount, value);
        }
        public string? CurrentInfo
        {
            get => currentInfo;

            set => SetProperty(ref currentInfo, value);
        }
        public int? TotalCount
        {
            get => totalCount;

            set => SetProperty(ref totalCount, value);
        }
        public double Percent
        {
            get => percent;

            set => SetProperty(ref percent, value);
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
            CurrentStateDescription = "完成";
            IsSplashNotVisible = true;
        }

        public SplashViewModel(ICookieService cookieService, IIntegrityCheckService integrityCheckService, IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger)
        {
            this.cookieService = cookieService;
            this.integrityCheckService = integrityCheckService;
            this.messenger = messenger;

            SetCookieCommand = asyncRelayCommandFactory.Create(SetCookieAsync);
            OpenUICommand = asyncRelayCommandFactory.Create(OpenUIAsync);
        }

        private async Task SetCookieAsync()
        {
            await cookieService.SetCookieAsync();
            IsCookieVisible = !cookieService.IsCookieAvailable;
        }

        private async Task OpenUIAsync()
        {
            CurrentStateDescription = "等待网络连接...";
            await Network.WaitConnectionAsync();
            CurrentStateDescription = "校验 Cookie 有效性...";
            await PerformCookieServiceCheckAsync();
            CurrentStateDescription = "校验 缓存资源 完整性...";
            await PerformIntegrityServiceCheckAsync();
            CurrentStateDescription = string.Empty;
            TrySendCompletedMessage();
        }

        private async Task PerformCookieServiceCheckAsync()
        {
            await cookieService.InitializeAsync();
            IsCookieVisible = !cookieService.IsCookieAvailable;
        }

        private async Task PerformIntegrityServiceCheckAsync()
        {
            Progress<IIntegrityCheckService.IIntegrityCheckState> progress = new();
            progress.ProgressChanged += (_, state) =>
            {
                CurrentCount = state.CurrentCount;
                Percent = (state.CurrentCount * 1D / TotalCount) ?? 0D;
                TotalCount = state.TotalCount;
                CurrentInfo = state.Info;
            };

            using (IntegrityChecking.Watch())
            {
                await integrityCheckService.CheckMetadataIntegrityAsync(progress);
            }
        }
    }
}
