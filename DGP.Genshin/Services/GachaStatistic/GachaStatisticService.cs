using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.Services.GachaStatistics.Statistics;
using DGP.Genshin.Common.Data.Behavior;
using DGP.Genshin.Common.Data.Privacy;
using DGP.Genshin.Common.Extensions.System;
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
        private readonly LocalGachaLogWorker localGachaLogWorker;

        /// <summary>
        /// 核心数据
        /// </summary>
        private readonly GachaDataCollection gachaDataCollection = new();

        #region LifeCycle
        public GachaStatisticService()
        {
            gachaDataCollection.UidAdded += GachaDataCollectionUidAdded;
            localGachaLogWorker = new(gachaDataCollection);
            this.Log("initialized");
        }

        private void GachaDataCollectionUidAdded(string uid)
        {
            bool showFullUid = Settings.SettingService.Instance.GetOrDefault(Settings.Setting.ShowFullUID, false);
            Uids.Add(new PrivateString(uid, PrivateString.DefaultMasker, showFullUid));
        }

        public void Initialize()
        {
            SelectedUid = Uids.First();
            SyncStatisticWithUid();
        }
        #endregion

        #region observables
        private Statistic? statistic;
        private PrivateString? selectedUid;
        private FetchProgress? fetchProgress;
        private SpecificBanner? selectedSpecificBanner;
        private bool canUserSwitchUid = true;
        private ObservableCollection<PrivateString> uids = new();

        /// <summary>
        /// 当前的统计信息
        /// </summary>
        public Statistic? Statistic { get => statistic; set => Set(ref statistic, value); }

        /// <summary>
        /// 当前选择的UID
        /// </summary>
        public PrivateString? SelectedUid
        {
            get => selectedUid; set
            {
                Set(ref selectedUid, value);
                if (CanUserSwitchUid)
                {
                    SyncStatisticWithUid();
                }
            }
        }

        /// <summary>
        /// 所有UID
        /// </summary>
        public ObservableCollection<PrivateString> Uids { get => uids; set => Set(ref uids, value); }

        /// <summary>
        /// UID切换状态锁
        /// 用来保证切换UID时无法再次切换
        /// </summary>
        public bool CanUserSwitchUid { get => canUserSwitchUid; set => Set(ref canUserSwitchUid, value); }

        /// <summary>
        /// 用于前台判断是否存在可展示的数据
        /// </summary>
        public bool HasNoData => gachaDataCollection.Count <= 0;

        /// <summary>
        /// 当前的获取进度
        /// </summary>
        public FetchProgress? FetchProgress { get => fetchProgress; set => Set(ref fetchProgress, value); }
        private void OnFetchProgressed(FetchProgress p)
        {
            FetchProgress = p;
        }
        /// <summary>
        /// 选定的特定池
        /// </summary>
        public SpecificBanner? SelectedSpecificBanner { get => selectedSpecificBanner; set => Set(ref selectedSpecificBanner, value); }

        #endregion

        public void PrepareDefaultStatistic()
        {

        }

        /// <summary>
        /// 获得当前的祈愿记录工作器
        /// </summary>
        /// <returns>如果无可用的Url则返回null</returns>
        public async Task<GachaLogWorker?> GetGachaLogWorkerAsync(GachaLogUrlMode mode)
        {
            string? url = await GachaLogUrlProvider.GetUrlAsync(mode);
            return url is null ? null : (new(url, gachaDataCollection));
        }

        public async void SyncStatisticWithUid()
        {
            await Task.Run(() =>
            {
                if (SelectedUid is null)
                {
                    return;
                }
                string? uid = SelectedUid.UnMaskedValue;
                Statistic = StatisticFactory.ToStatistic(gachaDataCollection[uid], uid);
                if (Statistic.SpecificBanners?.Count > 0)
                {
                    SelectedSpecificBanner = Statistic.SpecificBanners.First();
                }
            });
        }

        public async Task RefreshAsync(GachaLogUrlMode mode)
        {
            string GetFailHintByMode(GachaLogUrlMode mode)
            {
                return mode switch
                {
                    GachaLogUrlMode.GameLogFile => "请在游戏中打开祈愿历史记录页面后尝试刷新",
                    GachaLogUrlMode.ManualInput => "请重新输入有效的Url",
                    _ => string.Empty,
                };
            }

            CanUserSwitchUid = false;
            string? url = await GachaLogUrlProvider.GetUrlAsync(mode);
            if (url is null)
            {
                await new ContentDialog()
                {
                    Title = "获取祈愿记录失败",
                    Content = GetFailHintByMode(mode),
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
            else
            {
                if (!await RefreshInternalAsync(mode))
                {
                    await new ContentDialog()
                    {
                        Title = "获取祈愿配置信息失败",
                        Content = "可能是验证密钥已过期",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
            }
            CanUserSwitchUid = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns>卡池配置是否可用</returns>
        private async Task<bool> RefreshInternalAsync(GachaLogUrlMode mode)
        {
            GachaLogWorker? worker = await GetGachaLogWorkerAsync(mode);
            if (worker is null)
            {
                //TODO 提示用户获取失败
                return false;
            }
            bool isGachaConfigAvailable = await FetchGachaLogsAsync(worker);
            if (Statistic != null)
            {
                SyncStatisticWithUid();
            }
            FetchProgress = null;
            return isGachaConfigAvailable;
        }

        private async Task<bool> FetchGachaLogsAsync(GachaLogWorker worker)
        {
            //gacha config can be null while authkey timeout
            if (worker.GachaConfig != null && worker.GachaConfig.Types != null)
            {
                worker.OnFetchProgressed += OnFetchProgressed;
                foreach (ConfigType pool in worker.GachaConfig.Types)
                {
                    await worker.FetchGachaLogIncrementAsync(pool);
                }
                worker.OnFetchProgressed -= OnFetchProgressed;
                localGachaLogWorker.SaveAllLogs();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 导出数据到Excel
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task ExportDataToExcelAsync(string path)
        {
            if (selectedUid is null)
            {
                throw new InvalidOperationException("无UID");
            }
            await Task.Run(() => localGachaLogWorker.SaveLocalGachaDataToExcel(selectedUid.UnMaskedValue, path));
        }

        public async Task ImportFromGenshinGachaExportAsync(string path)
        {
            if (!await Task.Run(() => localGachaLogWorker.ImportFromGenshinGachaExport(path)))
            {
                await new ContentDialog()
                {
                    Title = "导入祈愿记录失败",
                    Content = "选择的文件内部格式不正确",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
        }
    }
}