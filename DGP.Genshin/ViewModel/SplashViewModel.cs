using CommunityToolkit.Mvvm.Messaging;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Message.Internal;
using DGP.Genshin.Service.Abstraction;
using DGP.Genshin.Service.Abstraction.IntegrityCheck;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using Snap.Data.Primitive;
using Snap.Net.Networking;
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

        private bool isCookieVisible = false;
        private bool isSplashNotVisible = false;
        private string currentStateDescription = "初始化...";
        private int currentCount;
        private string? currentInfo;
        private int? totalCount;
        private double percent;

        /// <summary>
        /// 构造一个新的启动界面视图模型
        /// </summary>
        /// <param name="cookieService">cookie服务</param>
        /// <param name="integrityCheckService">完整性检查服务</param>
        /// <param name="asyncRelayCommandFactory">异步命令工厂</param>
        /// <param name="messenger">消息器</param>
        public SplashViewModel(
            ICookieService cookieService,
            IIntegrityCheckService integrityCheckService,
            IAsyncRelayCommandFactory asyncRelayCommandFactory,
            IMessenger messenger)
        {
            this.cookieService = cookieService;
            this.integrityCheckService = integrityCheckService;
            this.messenger = messenger;

            this.SetCookieCommand = asyncRelayCommandFactory.Create(this.SetCookieAsync);
            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
        }

        /// <summary>
        /// 指示设置cookie功能是否可见
        /// </summary>
        public bool IsCookieVisible
        {
            get => this.isCookieVisible;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.isCookieVisible, value, this.TrySendCompletedMessage);
        }

        /// <summary>
        /// 设置为 <see cref="true"/> 以触发淡入主界面动画
        /// </summary>
        public bool IsSplashNotVisible
        {
            get => this.isSplashNotVisible;

            set => this.SetProperty(ref this.isSplashNotVisible, value);
        }

        /// <summary>
        /// 当前状态提示文本
        /// </summary>
        public string CurrentStateDescription
        {
            get => this.currentStateDescription;

            set => this.SetProperty(ref this.currentStateDescription, value);
        }

        /// <summary>
        /// 当前检查的个数
        /// </summary>
        public int CurrentCount
        {
            get => this.currentCount;

            set => this.SetProperty(ref this.currentCount, value);
        }

        /// <summary>
        /// 当前检查的提示文本
        /// </summary>
        public string? CurrentInfo
        {
            get => this.currentInfo;

            set => this.SetProperty(ref this.currentInfo, value);
        }

        /// <summary>
        /// 检查的总个数
        /// </summary>
        public int? TotalCount
        {
            get => this.totalCount;

            set => this.SetProperty(ref this.totalCount, value);
        }

        /// <summary>
        /// 进度
        /// </summary>
        public double Percent
        {
            get => this.percent;

            set => this.SetProperty(ref this.percent, value);
        }

        /// <summary>
        /// 完整性检查监视器
        /// </summary>
        public WorkWatcher IntegrityChecking { get; set; } = new();

        /// <summary>
        /// 打开界面触发的命令
        /// </summary>
        public ICommand OpenUICommand { get; }

        /// <summary>
        /// 设置Cookie命令
        /// </summary>
        public ICommand SetCookieCommand { get; }

        /// <summary>
        /// 通知 <see cref="SplashViewModel"/> 结束初始化
        /// </summary>
        public void CompleteInitialization()
        {
            this.CurrentStateDescription = "完成";
            this.IsSplashNotVisible = true;
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

        [PropertyChangedCallback]
        private void TrySendCompletedMessage()
        {
            if (this.IsCookieVisible == false && this.integrityCheckService.IntegrityChecking.IsCompleted)
            {
                this.messenger.Send(new SplashInitializationCompletedMessage(this));
            }
        }
    }
}