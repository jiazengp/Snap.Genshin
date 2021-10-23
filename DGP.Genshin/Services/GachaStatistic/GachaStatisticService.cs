using DGP.Genshin.Models.MiHoYo.Gacha;
using DGP.Genshin.Models.MiHoYo.Gacha.Statistics;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Privacy;
using DGP.Snap.Framework.Extensions.System;
using DGP.Snap.Framework.Extensions.System.Windows.Threading;
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
        private GachaLogProvider? gachaLogProvider;
        private readonly object providerLocker = new();
        public static GachaLogUrlMode CurrentMode = GachaLogUrlMode.GameLogFile;
        private GachaLogProvider GachaLogProvider
        {
            get
            {
                if (gachaLogProvider == null)
                {
                    lock (providerLocker)
                    {
                        if (gachaLogProvider == null)
                        {
                            gachaLogProvider = new GachaLogProvider(this);
                        }
                    }
                }
                return gachaLogProvider;
            }
        }

        #region Initialization
        public GachaStatisticService()
        {
            Initialize();
            this.Log("initialized");
            OnSelectedUidChanged += SyncStatisticWithUidAsync;
        }

        public void Initialize()
        {
            LoadLocalData();
        }

        private async void LoadLocalData()
        {
            await Task.Run(() =>
            {
                LocalGachaLogProvider localProvider = GachaLogProvider.LocalGachaLogProvider;
                if (!HasNoData && SelectedUid is not null)
                {
                    Statistic = StatisticFactory.ToStatistic(localProvider.Data[SelectedUid.UnMaskedValue], SelectedUid.UnMaskedValue);
                    //cause we suppress the notify in ctor of LocalGachaProvider
                    OnPropertyChanged(nameof(SelectedUid));
                    //select default banner
                    if (Statistic.SpecificBanners?.Count > 0)
                    {
                        SelectedSpecificBanner = Statistic.SpecificBanners.First();
                    }
                }
            });
        }
        #endregion

        ~GachaStatisticService()
        {
            this.Log("uninitialized");
            UnInitialize();
        }
        public void UnInitialize()
        {
            gachaLogProvider = null;
        }

        #region observables
        private Statistic? statistic;
        private PrivateString? selectedUid;
        private bool hasData = true;
        private FetchProgress? fetchProgress;
        private SpecificBanner? selectedSpecificBanner;
        private bool canUserSwitchUid = true;

        public Statistic? Statistic { get => statistic; set => Set(ref statistic, value); }
        public PrivateString? SelectedUid
        {
            get => selectedUid; set
            {
                Set(ref selectedUid, value);
                if (CanUserSwitchUid)
                {
                    OnSelectedUidChanged?.Invoke();
                }
            }
        }

        public event Action OnSelectedUidChanged;
        public void SetSelectedUidSuppressSyncStatistic(PrivateString uid)
        {
            selectedUid = uid;
        }

        public ObservableCollection<PrivateString> Uids { get; set; } = new ObservableCollection<PrivateString>();
        public bool CanUserSwitchUid { get => canUserSwitchUid; set => Set(ref canUserSwitchUid, value); }
        public bool HasNoData { get => hasData; set => Set(ref hasData, value); }
        public FetchProgress? FetchProgress { get => fetchProgress; set => Set(ref fetchProgress, value); }
        public SpecificBanner? SelectedSpecificBanner { get => selectedSpecificBanner; set => Set(ref selectedSpecificBanner, value); }
        #endregion

        /// <summary>
        /// 切换UID上下文，保证UID相关的数据可用性,不包含卡池信息
        /// <para>此方法需要从主(UI)线程调用,所有调用结束后需调用<see cref="SyncStatisticWithUidAsync"/></para>
        /// </summary>
        /// <param name="uid">目标uid</param>
        /// <returns>是否创建了新的Uid</returns>
        public bool SwitchUidContext(string? uid)
        {
            if (uid is null)
            {
                return false;
            }
            //this uid is first time fetch
            if (!GachaLogProvider.LocalGachaLogProvider.Data.ContainsKey(uid))
            {
                GachaLogProvider.LocalGachaLogProvider.InitializeUser(uid);
                PrivateString pUid = new PrivateString(uid, PrivateString.DefaultMasker, Settings.SettingModel.Instance.ShowFullUID);
                AddOrIgnore(pUid);
                SelectedUid = pUid;
                return true;
            }
            //switch to uid
            if (SelectedUid?.UnMaskedValue != uid)
            {
                SelectedUid = Uids.FirstOrDefault(u => u.UnMaskedValue == uid);
            }
            //uid is the same
            return false;
        }

        public async void SyncStatisticWithUidAsync()
        {
            await Task.Run(() =>
            {
                if (SelectedUid is null)
                {
                    return;
                }
                LocalGachaLogProvider localProvider = GachaLogProvider.LocalGachaLogProvider;
                Statistic = StatisticFactory.ToStatistic(localProvider.Data[SelectedUid.UnMaskedValue], SelectedUid.UnMaskedValue);
                if (Statistic.SpecificBanners?.Count > 0)
                {
                    SelectedSpecificBanner = Statistic.SpecificBanners.First();
                }
                HasNoData = false;
            });
        }
        public void AddOrIgnore(PrivateString uid)
        {
            if (!Uids.Contains(uid))
            {
                App.Current.Invoke(() => Uids.Add(uid));
            }
        }

        public async Task RefreshAsync(GachaLogUrlMode mode)
        {
            CanUserSwitchUid = false;
            if (await GachaLogProvider.TryGetUrlAsync(mode))
            {
                if (!await RefreshInternalAsync())
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
            else
            {
                await new ContentDialog()
                {
                    Title = "获取祈愿记录失败",
                    Content = GetFailHintByMode(mode),
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
            CanUserSwitchUid = true;
        }

        private readonly Random random = new Random();
        private async Task<bool> RefreshInternalAsync()
        {
            GachaLogProvider.OnFetchProgressed += OnFetchProgressed;
            bool result = await Task.Run(async () =>
            {
                //gacha config can be null while authkey timeout or no url
                if (GachaLogProvider.GachaConfig != null && GachaLogProvider.GachaConfig.Types != null)
                {
                    foreach (ConfigType pool in GachaLogProvider.GachaConfig.Types)
                    {
                        GachaLogProvider.FetchGachaLogIncrement(pool);
                        await Task.Delay(random.Next(0, 1000));
                    }
                    GachaLogProvider.SaveAllLogs();
                    return true;
                }
                else
                {
                    return false;
                }
            });
            if (Statistic != null)
            {
                SyncStatisticWithUidAsync();
            }
            GachaLogProvider.OnFetchProgressed -= OnFetchProgressed;
            FetchProgress = null;
            return result;
        }

        private string GetFailHintByMode(GachaLogUrlMode mode)
        {
            return mode switch
            {
                GachaLogUrlMode.GameLogFile => "请在游戏中打开祈愿历史记录页面后尝试刷新",
                GachaLogUrlMode.ManualInput => "请重新输入有效的Url",
                _ => string.Empty,
            };
        }

        private void OnFetchProgressed(FetchProgress p)
        {
            FetchProgress = p;
        }

        public async Task ExportDataToExcelAsync(string path)
        {
            await Task.Run(() => GachaLogProvider.LocalGachaLogProvider.SaveLocalGachaDataToExcel(path));
        }

        public async Task ImportFromGenshinGachaExportAsync(string path)
        {
            if (!await Task.Run(() => GachaLogProvider.LocalGachaLogProvider.ImportFromGenshinGachaExport(path)))
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