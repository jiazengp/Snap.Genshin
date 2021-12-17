using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Data.Privacy;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Messages;
using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.Services.GachaStatistics;
using DGP.Genshin.Services.GachaStatistics.Statistics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Win32;
using ModernWpf.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
    public class GachaStatisticViewModel : ObservableObject, IRecipient<GachaUidAddedMessage>
    {
        private readonly IGachaStatisticService gachaStatisticService;
        private readonly ISettingService settingService;

        private bool isRefreshing = false;
        private bool isSyncingUid = false;

        private Statistic? statistic;
        private PrivateString? selectedUid;
        private FetchProgress? fetchProgress;
        private SpecificBanner? selectedSpecificBanner;
        private ObservableCollection<PrivateString> uids = new();
        private bool isFullFetch;
        private IRelayCommand initializeCommand;
        private IAsyncRelayCommand gachaLogAutoFindCommand;
        private IAsyncRelayCommand gachaLogManualCommand;
        private IAsyncRelayCommand importFromUIGFJCommand;
        private IAsyncRelayCommand importFromUIGFWCommand;
        private IAsyncRelayCommand exportToUIGFWCommand;
        private IAsyncRelayCommand exportToUIGFJCommand;

        /// <summary>
        /// 当前的统计信息
        /// </summary>
        public Statistic? Statistic
        {
            get => statistic;
            set => SetProperty(ref statistic, value);
        }
        /// <summary>
        /// 当前选择的UID
        /// </summary>
        public PrivateString? SelectedUid
        {
            get => selectedUid;
            set
            {
                SetProperty(ref selectedUid, value);
                SyncStatisticWithUid();
            }
        }
        public async void SyncStatisticWithUid()
        {
            if (isSyncingUid)
            {
                return;
            }
            isSyncingUid = true;
            if (SelectedUid is null)
            {
                return;
            }
            string? uid = SelectedUid.UnMaskedValue;
            Statistic = await gachaStatisticService.GetStatisticAsync(uid);
            SelectedSpecificBanner = Statistic.SpecificBanners?.FirstOrDefault();
            isSyncingUid = false;
        }
        /// <summary>
        /// 所有UID
        /// </summary>
        public ObservableCollection<PrivateString> Uids
        {
            get => uids;
            set => SetProperty(ref uids, value);
        }
        /// <summary>
        /// 当前的获取进度
        /// </summary>
        public FetchProgress? FetchProgress
        {
            get => fetchProgress;
            set => SetProperty(ref fetchProgress, value);
        }
        /// <summary>
        /// 选定的特定池
        /// </summary>
        public SpecificBanner? SelectedSpecificBanner
        {
            get => selectedSpecificBanner;
            set => SetProperty(ref selectedSpecificBanner, value);
        }
        public bool IsFullFetch
        {
            get => isFullFetch;
            set => SetProperty(ref isFullFetch, value);
        }
        public IRelayCommand InitializeCommand
        {
            get => initializeCommand;
            [MemberNotNull(nameof(initializeCommand))]
            set => SetProperty(ref initializeCommand, value);
        }
        public IAsyncRelayCommand GachaLogAutoFindCommand
        {
            get => gachaLogAutoFindCommand;
            [MemberNotNull(nameof(gachaLogAutoFindCommand))]
            set => SetProperty(ref gachaLogAutoFindCommand, value);
        }
        public IAsyncRelayCommand GachaLogManualCommand
        {
            get => gachaLogManualCommand;
            [MemberNotNull(nameof(gachaLogManualCommand))]
            set => SetProperty(ref gachaLogManualCommand, value);
        }
        public IAsyncRelayCommand ImportFromUIGFJCommand
        {
            get => importFromUIGFJCommand;
            [MemberNotNull(nameof(importFromUIGFJCommand))]
            set => SetProperty(ref importFromUIGFJCommand, value);
        }
        public IAsyncRelayCommand ImportFromUIGFWCommand
        {
            get => importFromUIGFWCommand;
            [MemberNotNull(nameof(importFromUIGFWCommand))]
            set => SetProperty(ref importFromUIGFWCommand, value);
        }
        public IAsyncRelayCommand ExportToUIGFWCommand
        {
            get => exportToUIGFWCommand;
            [MemberNotNull(nameof(exportToUIGFWCommand))]
            set => SetProperty(ref exportToUIGFWCommand, value);
        }
        public IAsyncRelayCommand ExportToUIGFJCommand
        {
            get => exportToUIGFJCommand;
            [MemberNotNull(nameof(exportToUIGFJCommand))]
            set => SetProperty(ref exportToUIGFJCommand, value);
        }
        public GachaStatisticViewModel(IGachaStatisticService gachaStatisticService, ISettingService settingService)
        {
            this.gachaStatisticService = gachaStatisticService;
            Uids = new(gachaStatisticService.GetUids());
            this.settingService = settingService;


            InitializeCommand = new RelayCommand(() => { SelectedUid = Uids.FirstOrDefault(); });
            GachaLogAutoFindCommand = new AsyncRelayCommand(RefreshByAutoFindModeAsync);
            GachaLogManualCommand = new AsyncRelayCommand(RefreshByManualAsync);
            ImportFromUIGFJCommand = new AsyncRelayCommand(ImportFromUIGFJAsync);
            ImportFromUIGFWCommand = new AsyncRelayCommand(ImportFromUIGFWAsync);
            ExportToUIGFWCommand = new AsyncRelayCommand(ExportToUIGFWAsync);
            ExportToUIGFJCommand = new AsyncRelayCommand(ExportToUIGFJAsync);
        }

        private async Task ExportToUIGFJAsync()
        {
            if (SelectedUid is null)
            {
                return;
            }
            SaveFileDialog dialog = new()
            {
                Filter = "JS对象简谱文件|*.json",
                Title = "保存到文件",
                ValidateNames = true,
                CheckPathExists = true,
                FileName = $"{SelectedUid.UnMaskedValue}.json"
            };
            if (dialog.ShowDialog() == true)
            {
                await gachaStatisticService.ExportDataToJsonAsync(dialog.FileName, SelectedUid.UnMaskedValue);
                await new ContentDialog
                {
                    Title = "导出祈愿记录完成",
                    Content = $"祈愿记录已导出至 {dialog.SafeFileName}",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
        }

        private async Task ExportToUIGFWAsync()
        {
            if (SelectedUid is null)
            {
                return;
            }
            SaveFileDialog dialog = new()
            {
                Filter = "Excel 工作簿|*.xlsx",
                Title = "保存到表格",
                ValidateNames = true,
                CheckPathExists = true,
                FileName = $"{SelectedUid.UnMaskedValue}.xlsx"
            };
            if (dialog.ShowDialog() == true)
            {
                this.Log("try to export to excel");
                await gachaStatisticService.ExportDataToExcelAsync(dialog.FileName, SelectedUid.UnMaskedValue);
                await new ContentDialog
                {
                    Title = "导出祈愿记录完成",
                    Content = $"祈愿记录已导出至 {dialog.SafeFileName}",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
        }

        private async Task ImportFromUIGFWAsync()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Excel 工作簿|*.xlsx",
                Title = "从 可交换统一格式祈愿记录工作簿 文件导入",
                Multiselect = false,
                CheckFileExists = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                await gachaStatisticService.ImportFromUIGFWAsync(openFileDialog.FileName);
            }
        }

        private async Task ImportFromUIGFJAsync()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "JS对象简谱文件|*.json",
                Title = "从 可交换统一格式祈愿记录 Json文件导入",
                Multiselect = false,
                CheckFileExists = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                await gachaStatisticService.ImportFromUIGFJAsync(openFileDialog.FileName);
            }
        }

        private async Task RefreshByManualAsync()
        {
            if (!isRefreshing)
            {
                isRefreshing = true;
                (_, string? uid) = await gachaStatisticService.RefreshAsync(GachaLogUrlMode.ManualInput, OnFetchProgressed, IsFullFetch);
                FetchProgress = null;
                SelectedUid = Uids.FirstOrDefault(u => u.UnMaskedValue == uid);

                isRefreshing = false;
            }
        }

        private async Task RefreshByAutoFindModeAsync()
        {
            if (!isRefreshing)
            {
                isRefreshing = true;
                (_, string? uid) = await gachaStatisticService.RefreshAsync(GachaLogUrlMode.GameLogFile, OnFetchProgressed, IsFullFetch);
                FetchProgress = null;
                SelectedUid = Uids.FirstOrDefault(u => u.UnMaskedValue == uid);
                isRefreshing = false;
            }
        }

        private void OnFetchProgressed(FetchProgress p)
        {
            FetchProgress = p;
        }

        private void GachaDataCollectionUidSyncRequested(string uid)
        {
            SelectedUid = Uids.FirstOrDefault(u => u.UnMaskedValue == uid);
        }

        public void Receive(GachaUidAddedMessage message)
        {
            string uid = message.Value;
            this.Log($"uid {uid} added");
            bool showFullUid = settingService.GetOrDefault(Setting.ShowFullUID, false);
            App.Current.Dispatcher.Invoke(() => Uids.Add(new PrivateString(uid, PrivateString.DefaultMasker, showFullUid)));
        }
    }
}