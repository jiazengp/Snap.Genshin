using DGP.Genshin.DataModel.GachaStatistic;
using DGP.Genshin.DataModel.GachaStatistic.Banner;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Helper;
using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.Service.Abstraction.GachaStatistic;
using DGP.Genshin.Service.GachaStatistic;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Threading;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class GachaStatisticViewModel : ObservableObject2
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
            get => this.statistic;

            set => this.SetProperty(ref this.statistic, value);
        }
        /// <summary>
        /// 当前选择的UID
        /// </summary>
        public UidGachaData? SelectedUserGachaData
        {
            get => this.selectedUserGachaData;

            set => this.SetPropertyAndCallbackOverridePropertyState(ref this.selectedUserGachaData, value, this.SyncStatisticWithUid);
        }
        /// <summary>
        /// 同步统计数据与当前uid
        /// </summary>
        [PropertyChangedCallback]
        private void SyncStatisticWithUid()
        {
            this.Log($"try read:{this.SelectedUserGachaData}");
            if (this.SelectedUserGachaData is not null)
            {
                this.Statistic = this.gachaStatisticService.GetStatistic(this.UserGachaDataCollection, this.SelectedUserGachaData.Uid);
                this.SelectedSpecificBanner = this.Statistic.SpecificBanners?.FirstOrDefault();
            }
        }
        /// <summary>
        /// 所有UID
        /// </summary>
        public GachaDataCollection UserGachaDataCollection
        {
            get => this.userGachaDataCollection;

            set => this.SetProperty(ref this.userGachaDataCollection, value);
        }
        /// <summary>
        /// 当前的获取进度
        /// </summary>
        public FetchProgress? FetchProgress
        {
            get => this.fetchProgress;

            set => this.SetProperty(ref this.fetchProgress, value);
        }
        /// <summary>
        /// 选定的特定池
        /// </summary>
        public SpecificBanner? SelectedSpecificBanner
        {
            get => this.selectedSpecificBanner;

            set => this.SetProperty(ref this.selectedSpecificBanner, value);
        }
        public bool IsFullFetch
        {
            get => this.isFullFetch;

            set => this.SetProperty(ref this.isFullFetch, value);
        }

        public ICommand OpenUICommand { get; }
        public ICommand GachaLogAutoFindCommand { get; }
        public ICommand GachaLogManualCommand { get; }
        public ICommand ImportFromUIGFJCommand { get; }
        public ICommand ExportToUIGFJCommand { get; }
        public ICommand ImportFromUIGFWCommand { get; }
        public ICommand ExportToUIGFWCommand { get; }
        public ICommand OpenGachaStatisticFolderCommand { get; }

        private IProgress<FetchProgress> Progress { get; }

        public GachaStatisticViewModel(IGachaStatisticService gachaStatisticService, IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            this.gachaStatisticService = gachaStatisticService;

            this.Progress = new Progress<FetchProgress>(this.OnFetchProgressed);

            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
            this.GachaLogAutoFindCommand = asyncRelayCommandFactory.Create(this.RefreshByAutoFindModeAsync);
            this.GachaLogManualCommand = asyncRelayCommandFactory.Create(this.RefreshByManualAsync);
            this.ImportFromUIGFJCommand = asyncRelayCommandFactory.Create(this.ImportFromUIGFJAsync);
            this.ImportFromUIGFWCommand = asyncRelayCommandFactory.Create(this.ImportFromUIGFWAsync);
            this.ExportToUIGFWCommand = asyncRelayCommandFactory.Create(this.ExportToUIGFWAsync);
            this.ExportToUIGFJCommand = asyncRelayCommandFactory.Create(this.ExportToUIGFJAsync);
            this.OpenGachaStatisticFolderCommand = new RelayCommand(this.OpenGachaStatisticFolder);
        }
        private async Task OpenUIAsync()
        {
            await this.gachaStatisticService.LoadLocalGachaDataAsync(this.UserGachaDataCollection);
            this.SelectedUserGachaData = this.UserGachaDataCollection.FirstOrDefault();
        }

        private async Task RefreshByAutoFindModeAsync()
        {
            if (this.taskPreventer.ShouldExecute)
            {
                try
                {
                    (bool isOk, string uid) = await this.gachaStatisticService.RefreshAsync(this.UserGachaDataCollection, GachaLogUrlMode.GameLogFile, this.Progress, this.IsFullFetch);
                    this.FetchProgress = null;
                    if (isOk)
                    {
                        this.SelectedUserGachaData = this.UserGachaDataCollection.FirstOrDefault(u => u.Uid == uid);
                    }
                }
                catch (Exception ex)
                {
                    this.Log(ex);
                }
                this.taskPreventer.Release();
            }
        }
        private async Task RefreshByManualAsync()
        {
            if (this.taskPreventer.ShouldExecute)
            {
                try
                {
                    (bool isOk, string uid) = await this.gachaStatisticService.RefreshAsync(this.UserGachaDataCollection, GachaLogUrlMode.ManualInput, this.Progress, this.IsFullFetch);
                    this.FetchProgress = null;
                    if (isOk)
                    {
                        this.SelectedUserGachaData = this.UserGachaDataCollection.FirstOrDefault(u => u.Uid == uid);
                    }
                }
                catch (Exception ex)
                {
                    this.Log(ex);
                }
                this.taskPreventer.Release();
            }
        }



        private void OnFetchProgressed(FetchProgress p)
        {
            this.FetchProgress = p;
        }
        private async Task ImportFromUIGFJAsync()
        {
            if (this.taskPreventer.ShouldExecute)
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
                    (bool isOk, string uid) = await this.gachaStatisticService.ImportFromUIGFJAsync(this.UserGachaDataCollection, openFileDialog.FileName);
                    if (!isOk)
                    {
                        await new ContentDialog()
                        {
                            Title = "导入祈愿记录失败",
                            Content = "文件不是UIGF格式，或支持的UIGF版本较低",
                            PrimaryButtonText = "确定",
                            DefaultButton = ContentDialogButton.Primary
                        }.ShowAsync();
                    }
                    else
                    {
                        this.SelectedUserGachaData = this.UserGachaDataCollection.FirstOrDefault(u => u.Uid == uid);
                    }
                }
                this.taskPreventer.Release();
            }
        }
        private async Task ExportToUIGFJAsync()
        {
            if (this.taskPreventer.ShouldExecute)
            {
                if (this.SelectedUserGachaData is null)
                {
                    return;
                }
                SaveFileDialog dialog = new()
                {
                    Filter = "JS对象简谱文件|*.json",
                    Title = "保存到文件",
                    ValidateNames = true,
                    CheckPathExists = true,
                    FileName = $"{this.SelectedUserGachaData.Uid}.json"
                };
                if (dialog.ShowDialog() is true)
                {
                    await this.gachaStatisticService.ExportDataToJsonAsync(this.UserGachaDataCollection, this.SelectedUserGachaData.Uid, dialog.FileName);
                    await new ContentDialog
                    {
                        Title = "导出祈愿记录完成",
                        Content = $"祈愿记录已导出至 {dialog.SafeFileName}",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
                this.taskPreventer.Release();
            }
        }
        private async Task ImportFromUIGFWAsync()
        {
            if (this.taskPreventer.ShouldExecute)
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
                    (bool isOk, string uid) = await this.gachaStatisticService.ImportFromUIGFWAsync(this.UserGachaDataCollection, openFileDialog.FileName);
                    if (!isOk)
                    {
                        await new ContentDialog()
                        {
                            Title = "导入失败",
                            Content = "文件不是UIGF格式，或支持的UIGF版本较低",
                            PrimaryButtonText = "确定",
                            DefaultButton = ContentDialogButton.Primary
                        }.ShowAsync();
                    }
                    else
                    {
                        this.SelectedUserGachaData = this.UserGachaDataCollection.FirstOrDefault(u => u.Uid == uid);
                    }
                }
                this.taskPreventer.Release();
            }
        }
        private async Task ExportToUIGFWAsync()
        {
            if (this.taskPreventer.ShouldExecute)
            {
                if (this.SelectedUserGachaData is null)
                {
                    return;
                }
                SaveFileDialog dialog = new()
                {
                    Filter = "Excel 工作簿|*.xlsx",
                    Title = "保存到表格",
                    ValidateNames = true,
                    CheckPathExists = true,
                    FileName = $"{this.SelectedUserGachaData.Uid}.xlsx"
                };
                if (dialog.ShowDialog() is true)
                {
                    this.Log("try to export to excel");
                    await this.gachaStatisticService.ExportDataToExcelAsync(this.UserGachaDataCollection, this.SelectedUserGachaData.Uid, dialog.FileName);
                    await new ContentDialog
                    {
                        Title = "导出祈愿记录完成",
                        Content = $"祈愿记录已导出至 {dialog.SafeFileName}",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
                this.taskPreventer.Release();
            }
        }
        private void OpenGachaStatisticFolder()
        {
            Process.Start("explorer.exe", PathContext.Locate("GachaStatistic"));
        }
    }
}