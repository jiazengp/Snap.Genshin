using DGP.Genshin.DataModel.GachaStatistic;
using DGP.Genshin.Helper;
using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.Service.Abstratcion;
using DGP.Genshin.Service.GachaStatistic;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Threading;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    public class GachaStatisticViewModel : ObservableObject2
    {
        private readonly IGachaStatisticService gachaStatisticService;

        private readonly TaskPreventer taskPreventer = new();

        private Statistic? statistic;
        private UidGachaData? selectedUserGachaData;
        private FetchProgress? fetchProgress;
        private SpecificBanner? selectedSpecificBanner;
        private GachaDataCollection userGachaDataCollection = new();
        private bool isFullFetch;

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
        public UidGachaData? SelectedUserGachaData
        {
            get => selectedUserGachaData;
            set => SetPropertyAndCallbackOverridePropertyState(ref selectedUserGachaData, value, SyncStatisticWithUid);
        }
        /// <summary>
        /// 同步统计数据与当前uid
        /// </summary>
        [PropertyChangedCallback]
        private async void SyncStatisticWithUid()
        {
            if (SelectedUserGachaData is not null)
            {
                Statistic = await gachaStatisticService.GetStatisticAsync(UserGachaDataCollection, SelectedUserGachaData.Uid);
                SelectedSpecificBanner = Statistic.SpecificBanners?.FirstOrDefault();
            }
        }
        /// <summary>
        /// 所有UID
        /// </summary>
        public GachaDataCollection UserGachaDataCollection
        {
            get => userGachaDataCollection;
            set => SetProperty(ref userGachaDataCollection, value);
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

        public ICommand OpenUICommand { get; }
        public ICommand GachaLogAutoFindCommand { get; }
        public ICommand GachaLogManualCommand { get; }
        public ICommand ImportFromUIGFJCommand { get; }
        public ICommand ImportFromUIGFWCommand { get; }
        public ICommand ExportToUIGFWCommand { get; }
        public ICommand ExportToUIGFJCommand { get; }
        public ICommand OpenGachaStatisticFolderCommand { get; }

        public GachaStatisticViewModel(IGachaStatisticService gachaStatisticService)
        {
            this.gachaStatisticService = gachaStatisticService;
            gachaStatisticService.LoadLocalGachaData(UserGachaDataCollection);

            OpenUICommand = new RelayCommand(() => { SelectedUserGachaData = UserGachaDataCollection.FirstOrDefault(); });
            GachaLogAutoFindCommand = new AsyncRelayCommand(RefreshByAutoFindModeAsync);
            GachaLogManualCommand = new AsyncRelayCommand(RefreshByManualAsync);
            ImportFromUIGFJCommand = new AsyncRelayCommand(ImportFromUIGFJAsync);
            ImportFromUIGFWCommand = new AsyncRelayCommand(ImportFromUIGFWAsync);
            ExportToUIGFWCommand = new AsyncRelayCommand(ExportToUIGFWAsync);
            ExportToUIGFJCommand = new AsyncRelayCommand(ExportToUIGFJAsync);
            OpenGachaStatisticFolderCommand = new RelayCommand(OpenGachaStatisticFolder);
        }

        private async Task RefreshByAutoFindModeAsync()
        {
            if (taskPreventer.ShouldExecute)
            {
                (bool isOk, string? uid) = await gachaStatisticService.RefreshAsync(UserGachaDataCollection, GachaLogUrlMode.GameLogFile, OnFetchProgressed, IsFullFetch);
                FetchProgress = null;
                if (isOk)
                {
                    SelectedUserGachaData = UserGachaDataCollection.FirstOrDefault(u => u.Uid == uid);
                }
                taskPreventer.Release();
            }
        }
        private async Task RefreshByManualAsync()
        {
            if (taskPreventer.ShouldExecute)
            {
                (bool isOk, string? uid) = await gachaStatisticService.RefreshAsync(UserGachaDataCollection, GachaLogUrlMode.ManualInput, OnFetchProgressed, IsFullFetch);
                FetchProgress = null;
                if (isOk)
                {
                    SelectedUserGachaData = UserGachaDataCollection.FirstOrDefault(u => u.Uid == uid);
                }
                taskPreventer.Release();
            }
        }
        private void OnFetchProgressed(FetchProgress p)
        {
            FetchProgress = p;
        }
        private async Task ImportFromUIGFJAsync()
        {
            if (taskPreventer.ShouldExecute)
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "JS对象简谱文件|*.json",
                    Title = "从 Json 文件导入",
                    Multiselect = false,
                    CheckFileExists = true
                };
                if (openFileDialog.ShowDialog() is true)
                {
                    if (!await gachaStatisticService.ImportFromUIGFJAsync(UserGachaDataCollection, openFileDialog.FileName))
                    {
                        await new ContentDialog()
                        {
                            Title = "导入祈愿记录失败",
                            Content = "选择的Json文件不是标准的可交换格式",
                            PrimaryButtonText = "确定",
                            DefaultButton = ContentDialogButton.Primary
                        }.ShowAsync();
                    }
                    else
                    {
                        SyncStatisticWithUid();
                    }
                }
                taskPreventer.Release();
            }
        }
        private async Task ImportFromUIGFWAsync()
        {
            if (taskPreventer.ShouldExecute)
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "Excel 工作簿|*.xlsx",
                    Title = "从 Excel 文件导入",
                    Multiselect = false,
                    CheckFileExists = true
                };
                if (openFileDialog.ShowDialog() is true)
                {
                    if (!await gachaStatisticService.ImportFromUIGFWAsync(UserGachaDataCollection, openFileDialog.FileName))
                    {
                        await new ContentDialog()
                        {
                            Title = "导入祈愿记录失败",
                            Content = "选择的Excel文件不是标准的可交换格式",
                            PrimaryButtonText = "确定",
                            DefaultButton = ContentDialogButton.Primary
                        }.ShowAsync();
                    }
                    else
                    {
                        SyncStatisticWithUid();
                    }
                }
                taskPreventer.Release();
            }
        }
        private async Task ExportToUIGFWAsync()
        {
            if (taskPreventer.ShouldExecute)
            {
                if (SelectedUserGachaData is null)
                {
                    return;
                }
                SaveFileDialog dialog = new()
                {
                    Filter = "Excel 工作簿|*.xlsx",
                    Title = "保存到表格",
                    ValidateNames = true,
                    CheckPathExists = true,
                    FileName = $"{SelectedUserGachaData.Uid}.xlsx"
                };
                if (dialog.ShowDialog() is true)
                {
                    this.Log("try to export to excel");
                    await gachaStatisticService.ExportDataToExcelAsync(UserGachaDataCollection, SelectedUserGachaData.Uid, dialog.FileName);
                    await new ContentDialog
                    {
                        Title = "导出祈愿记录完成",
                        Content = $"祈愿记录已导出至 {dialog.SafeFileName}",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
                taskPreventer.Release();
            }
        }
        private async Task ExportToUIGFJAsync()
        {
            if (taskPreventer.ShouldExecute)
            {
                if (SelectedUserGachaData is null)
                {
                    return;
                }
                SaveFileDialog dialog = new()
                {
                    Filter = "JS对象简谱文件|*.json",
                    Title = "保存到文件",
                    ValidateNames = true,
                    CheckPathExists = true,
                    FileName = $"{SelectedUserGachaData.Uid}.json"
                };
                if (dialog.ShowDialog() is true)
                {
                    await gachaStatisticService.ExportDataToJsonAsync(UserGachaDataCollection, SelectedUserGachaData.Uid, dialog.FileName);
                    await new ContentDialog
                    {
                        Title = "导出祈愿记录完成",
                        Content = $"祈愿记录已导出至 {dialog.SafeFileName}",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
                taskPreventer.Release();
            }
        }
        private void OpenGachaStatisticFolder()
        {
            Process.Start("explorer.exe", PathContext.Locate("GachaStatistic"));
        }
    }
}