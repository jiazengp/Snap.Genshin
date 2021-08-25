using DGP.Genshin.Models.MiHoYo.Gacha;
using DGP.Genshin.Models.MiHoYo.Gacha.Statistics;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Privacy;
using ModernWpf.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.GachaStatistic
{
    /// <summary>
    /// 抽卡记录服务
    /// </summary>
    public class GachaStatisticService : Observable
    {
        private GachaLogProvider gachaLogProvider;
        private readonly object providerLocker = new object();
        private GachaLogProvider GachaLogProvider
        {
            get
            {
                if (this.gachaLogProvider == null)
                {
                    lock (this.providerLocker)
                    {
                        if (this.gachaLogProvider == null)
                        {
                            this.gachaLogProvider = new GachaLogProvider(this);
                        }
                    }
                }
                return this.gachaLogProvider;
            }
        }

        #region Initialization
        public GachaStatisticService()
        {
            Initialize();
            OnSelectedUidChanged += SyncStatisticWithUidAsync;
        }

        private void SyncStatistic(PrivateString obj) => throw new NotImplementedException();
        public void Initialize() => LoadLocalData();
        private async void LoadLocalData()
        {
            await Task.Run(() =>
            {
                LocalGachaLogProvider localProvider = this.GachaLogProvider.LocalGachaLogProvider;
                if (!HasNoData)
                {
                    this.Statistic = StatisticFactory.ToStatistic(localProvider.Data[this.SelectedUid.UnMaskedValue], this.SelectedUid.UnMaskedValue);
                    //select default banner
                    if (this.Statistic.SpecificBanners.Count > 0)
                    {
                        this.SelectedSpecificBanner = this.Statistic.SpecificBanners.First();
                    }
                }
            });
        }
        #endregion

        ~GachaStatisticService()
        {
            UnInitialize();
        }
        public void UnInitialize() => this.gachaLogProvider = null;

        #region observables
        private Statistic statistic;
        private PrivateString selectedUid;
        private bool hasData = true;
        private FetchProgress fetchProgress;
        private SpecificBanner selectedSpecificBanner;
        private bool canUserSwitchUid=true;

        public Statistic Statistic { get => this.statistic; set => Set(ref this.statistic, value); }
        public PrivateString SelectedUid
        {
            get => this.selectedUid; set
            {
                Set(ref this.selectedUid, value);
                if (CanUserSwitchUid)
                {
                    OnSelectedUidChanged?.Invoke();
                }
            }
        }

        public event Action OnSelectedUidChanged;
        public void SetSelectedUidSuppressSyncStatistic(PrivateString uid) =>
            this.selectedUid = uid;
        public ObservableCollection<PrivateString> Uids { get; set; } = new ObservableCollection<PrivateString>();
        public bool CanUserSwitchUid { get => canUserSwitchUid; set => Set(ref canUserSwitchUid, value); }
        public bool HasNoData { get => this.hasData; set => Set(ref this.hasData, value); }
        public FetchProgress FetchProgress { get => this.fetchProgress; set => Set(ref this.fetchProgress, value); }
        public SpecificBanner SelectedSpecificBanner { get => this.selectedSpecificBanner; set => Set(ref this.selectedSpecificBanner, value); }
        #endregion

        /// <summary>
        /// 切换UID上下文，保证UID相关的数据可用性,不包含卡池信息
        /// <para>此方法需要从主(UI)线程调用,所有调用结束后需调用<see cref="SyncStatisticWithUidAsync"/></para>
        /// </summary>
        /// <param name="uid">目标uid</param>
        /// <returns>是否创建了新的Uid</returns>
        public bool SwitchUidContext(string uid)
        {
            //this uid is first time fetch
            if (!this.GachaLogProvider.LocalGachaLogProvider.Data.ContainsKey(uid))
            {
                this.GachaLogProvider.LocalGachaLogProvider.InitializeUser(uid);
                PrivateString pUid = new PrivateString(uid, PrivateString.DefaultMasker, Settings.SettingModel.Instance.ShowFullUID);
                AddOrIgnore(pUid);
                this.SelectedUid = pUid;
                return true;
            }
            //switch to uid
            if (this.SelectedUid.UnMaskedValue != uid)
            {
                this.SelectedUid = this.Uids.FirstOrDefault(u => u.UnMaskedValue == uid);
            }
            //uid is the same
            return false;
        }

        public async void SyncStatisticWithUidAsync()
        {
            await Task.Run(() =>
            {
                LocalGachaLogProvider localProvider = this.GachaLogProvider.LocalGachaLogProvider;
                this.Statistic = StatisticFactory.ToStatistic(localProvider.Data[this.SelectedUid.UnMaskedValue], this.SelectedUid.UnMaskedValue);
                if (this.Statistic.SpecificBanners.Count > 0)
                {
                    this.SelectedSpecificBanner = this.Statistic.SpecificBanners.First();
                    _ = this.SelectedSpecificBanner;
                }
                this.HasNoData = false;
            });
        }
        public void AddOrIgnore(PrivateString uid)
        {
            if (!this.Uids.Contains(uid))
                this.Uids.Add(uid);
        }
        public async void Refresh()
        {
            this.CanUserSwitchUid = false;
            if (this.GachaLogProvider.TryFindUrlInLogFile())
            {
                this.GachaLogProvider.OnFetchProgressed += OnFetchProgressed;
                await Task.Run(() =>
                {
                    //gacha config can be null
                    foreach (ConfigType pool in this.GachaLogProvider.GachaConfig.Types)
                    {
                        this.GachaLogProvider.FetchGachaLogIncrement(pool);
                    }
                    this.GachaLogProvider.SaveAllLogs();
                });
                SyncStatisticWithUidAsync();
                this.GachaLogProvider.OnFetchProgressed -= OnFetchProgressed;
                this.FetchProgress = null;
            }
            else
            {
                await new ContentDialog()
                {
                    Title = "获取祈愿记录失败",
                    Content = "请在游戏中打开祈愿历史记录页面后尝试刷新",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
            this.CanUserSwitchUid = true;
        }
        private void OnFetchProgressed(FetchProgress p) => this.FetchProgress = p;

        public async Task ExportDataToExcelAsync(string path) =>
            await Task.Run(() => this.GachaLogProvider.LocalGachaLogProvider.SaveLocalGachaDataToExcel(path));

        public async Task ImportFromGenshinGachaExportAsync(string path) =>
            await Task.Run(() => this.GachaLogProvider.LocalGachaLogProvider.ImportFromGenshinGachaExport(path));

    }
}