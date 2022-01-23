using DGP.Genshin.Messages;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using Snap.Net.Networking;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModels
{
    /// <summary>
    /// 启动界面视图模型
    /// </summary>
    [ViewModel(InjectAs.Transient)]
    public class SplashViewModel : ObservableObject2
    {
        private readonly ICookieService cookieService;
        private readonly IIntegrityCheckService integrityCheckService;

        private bool isCookieVisible = false;
        private bool isSplashNotVisible = false;
        private string currentStateDescription = "初始化...";
        private int currentCount;
        private string? currentInfo;
        private int? totalCount;
        private double percent;
        private bool isCheckingIntegrity;

        public bool IsCookieVisible
        {
            get => isCookieVisible;
            set => SetPropertyAndCallbackOnCompletion(ref isCookieVisible, value, TrySendCompletedMessage);
        }

        [PropertyChangedCallback]
        private void TrySendCompletedMessage()
        {
            if (IsCookieVisible == false && integrityCheckService.IntegrityCheckCompleted)
            {
                App.Messenger.Send(new SplashInitializationCompletedMessage(this));
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
        public bool IsCheckingIntegrity
        {
            get => isCheckingIntegrity;
            set => SetProperty(ref isCheckingIntegrity, value);
        }

        public ICommand OpenUICommand { get; }
        public ICommand SetCookieCommand { get; }

        public SplashViewModel(ICookieService cookieService, IIntegrityCheckService integrityCheckService)
        {
            this.cookieService = cookieService;
            this.integrityCheckService = integrityCheckService;

            SetCookieCommand = new AsyncRelayCommand(SetCookieAsync);
            OpenUICommand = new AsyncRelayCommand(OpenUIAsync);
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
            IsCheckingIntegrity = true;
            await integrityCheckService.CheckMetadataIntegrityAsync(state =>
            {
                CurrentCount = state.CurrentCount;
                Percent = (state.CurrentCount * 1D / TotalCount) ?? 0D;
                TotalCount = state.TotalCount;
                CurrentInfo = state.Info;
            });
            IsCheckingIntegrity = false;
        }
    }
}
