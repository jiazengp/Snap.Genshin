using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Net;
using DGP.Genshin.Messages;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DGP.Genshin.ViewModels
{
    /// <summary>
    /// 启动界面视图模型
    /// </summary>
    [ViewModel(ViewModelType.Transient)]
    [Send(typeof(SplashInitializationCompletedMessage))]
    public class SplashViewModel : ObservableObject
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
        private IAsyncRelayCommand openUICommand;
        private IAsyncRelayCommand setCookieCommand;

        public bool IsCookieVisible
        {
            get => isCookieVisible; set
            {
                SetProperty(ref isCookieVisible, value);
                TrySendCompletedMessage();
            }
        }

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
        public string CurrentStateDescription { get => currentStateDescription; set => SetProperty(ref currentStateDescription, value); }
        public int CurrentCount { get => currentCount; set => SetProperty(ref currentCount, value); }
        public string? CurrentInfo { get => currentInfo; set => SetProperty(ref currentInfo, value); }

        public int? TotalCount { get => totalCount; set => SetProperty(ref totalCount, value); }

        public double Percent { get => percent; set => SetProperty(ref percent, value); }
        public IAsyncRelayCommand OpenUICommand
        {
            get => openUICommand;
            [MemberNotNull(nameof(openUICommand))]
            set => SetProperty(ref openUICommand, value);
        }
        public IAsyncRelayCommand SetCookieCommand
        {
            get => setCookieCommand;
            [MemberNotNull(nameof(setCookieCommand))]
            set => SetProperty(ref setCookieCommand, value);
        }

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
            TrySendCompletedMessage();
        }

        private async Task PerformCookieServiceCheckAsync()
        {
            await cookieService.InitializeAsync();
            IsCookieVisible = !cookieService.IsCookieAvailable;
        }

        private async Task PerformIntegrityServiceCheckAsync()
        {
            await integrityCheckService.CheckMetadataIntegrityAsync(state =>
            {
                CurrentCount = state.CurrentCount;
                Percent = (state.CurrentCount * 1D / TotalCount) ?? 0D;
                TotalCount = state.TotalCount;
                CurrentInfo = state.Info;
            });
        }
    }
}
