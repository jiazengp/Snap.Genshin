using DGP.Genshin.Models.MiHoYo.Gacha;
using DGP.Genshin.Models.MiHoYo.Gacha.Statistics;
using DGP.Snap.Framework.Data.Behavior;
using ModernWpf.Controls;
using System;
using System.Collections.ObjectModel;
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

        public GachaStatisticService()
        {
            this.Initialize();
        }
        ~GachaStatisticService()
        {
            this.UnInitialize();
        }

        public void Initialize() => this.LoadLocalData();
        public void UnInitialize() => this.gachaLogProvider = null;

        #region observables
        private Statistic statistic;
        private string selectedUid;
        private bool hasData = true;
        private FetchProgress fetchProgress;

        public Statistic Statistic { get => this.statistic; set => this.Set(ref this.statistic, value); }
        public string SelectedUid
        {
            get => this.selectedUid; set => this.Set(ref this.selectedUid, value);
        }
        public ObservableCollection<string> Uids { get; set; } = new ObservableCollection<string>();
        public bool HasNoData { get => this.hasData; set => this.Set(ref this.hasData, value); }
        public FetchProgress FetchProgress { get => this.fetchProgress; set => this.Set(ref this.fetchProgress, value); }
        #endregion

        public void AddOrIgnore(string uid)
        {
            if (!this.Uids.Contains(uid))
                this.Uids.Add(uid);
        }
        public async void Refresh()
        {
            if (this.GachaLogProvider.TryFindUrlInLogFile())
            {
                this.GachaLogProvider.OnFetchProgressed += this.OnFetchProgressed;
                await Task.Run(() =>
                {
                    foreach (ConfigType pool in this.GachaLogProvider.GachaConfig.Types)
                    {
                        this.GachaLogProvider.FetchGachaLogIncrement(pool);
                    }
                    this.GachaLogProvider.SaveAllLogs();
                }).ContinueWith((t) =>
                {
                    this.Statistic = StatisticFactory.ToStatistic(this.GachaLogProvider.LocalGachaLogProvider.Data[this.SelectedUid], this.SelectedUid);
                    this.HasNoData = false;
                });
                this.GachaLogProvider.OnFetchProgressed -= this.OnFetchProgressed;
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
        }
        private void OnFetchProgressed(FetchProgress p) => this.FetchProgress = p;
        private async void LoadLocalData()
        {
            await Task.Run(() =>
            {
                LocalGachaLogProvider localProvider = this.GachaLogProvider.LocalGachaLogProvider;
                if (localProvider.Data.Count > 0)
                {
                    this.Statistic = StatisticFactory.ToStatistic(localProvider.Data[this.SelectedUid], this.SelectedUid);
                    this.HasNoData = false;
                }
            });
        }

        public async Task ExportDataToExcelAsync()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            await Task.Run(() => this.GachaLogProvider.LocalGachaLogProvider.SaveLocalGachaDataToExcel($@"{path}\{this.SelectedUid}.xlsx"));
        }
    }
}