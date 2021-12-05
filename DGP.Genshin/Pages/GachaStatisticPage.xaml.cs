using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Services.GachaStatistics;
using Microsoft.Win32;
using ModernWpf.Controls;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// GachaStatisticPage.xaml 的交互逻辑
    /// </summary>
    public partial class GachaStatisticPage : Page
    {
        private GachaStatisticService? Service { get; set; }

        public GachaStatisticPage()
        {
            InitializeComponent();
            this.Log("initialized");
        }

        private bool isRefreshing = false;

        private async void AutoFindAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isRefreshing)
            {
                if (Service is not null)
                {
                    isRefreshing = true;
                    await Service.RefreshAsync(GachaLogUrlMode.GameLogFile, FullSwitch.IsOn);
                    isRefreshing = false;
                }
            }
        }

        private async void ManualInputUrlAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isRefreshing)
            {
                if (Service is not null)
                {
                    isRefreshing = true;
                    await Service.RefreshAsync(GachaLogUrlMode.ManualInput, FullSwitch.IsOn);
                    isRefreshing = false;
                }
            }
        }

        private async void ImportFromGenshinGachaExportAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "JS对象简谱文件|*.json",
                Title = "从 Genshin Gacha Export 记录文件导入",
                Multiselect = false,
                CheckFileExists = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Log("try to import from genshin gacha export");

                if (Service is not null)
                {
                    await Service.ImportFromGenshinGachaExportAsync(openFileDialog.FileName);
                }
            }
        }

        private async void ImportFromKeqingNiuzaAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "JS对象简谱文件|*.json",
                Title = "从 刻记牛杂店 记录文件导入",
                Multiselect = false,
                CheckFileExists = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Log("try to import from keqing niuza");

                if (Service is not null)
                {
                    await Service.ImportFromKeqingNiuzaAsync(openFileDialog.FileName);
                }
            }
        }

        private async void ImportFromUIGFWAppBarButtonClick(object sender, RoutedEventArgs e)
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
                this.Log("try to import from UIGF.W");

                if (Service is not null)
                {
                    await Service.ImportFromUIGFWAsync(openFileDialog.FileName);
                }
            }
        }
        private async void ImportFromUIGFJAppBarButtonClick(object sender, RoutedEventArgs e)
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
                this.Log("try to import from UIGF.J");

                if (Service is not null)
                {
                    await Service.ImportFromUIGFJAsync(openFileDialog.FileName);
                }
            }
        }

        private async void ExportExcelAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new()
            {
                Filter = "Excel 工作簿|*.xlsx",
                Title = "保存到表格",
                ValidateNames = true,
                CheckPathExists = true,
                FileName = $"{Service?.SelectedUid?.UnMaskedValue}.xlsx"
            };
            if (dialog.ShowDialog() == true)
            {
                this.Log("try to export to excel");
                if (Service is not null)
                {
                    await Service.ExportDataToExcelAsync(dialog.FileName);
                    await new ContentDialog
                    {
                        Title = "导出祈愿记录完成",
                        Content = $"祈愿记录已导出至 {dialog.SafeFileName}",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
            }
        }
        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new()
            {
                Filter = "JS对象简谱文件|*.json",
                Title = "保存到文件",
                ValidateNames = true,
                CheckPathExists = true,
                FileName = $"{Service?.SelectedUid?.UnMaskedValue}.json"
            };
            if (dialog.ShowDialog() == true)
            {
                this.Log("try to export to json");
                if (Service is not null)
                {
                    await Service.ExportDataToJsonAsync(dialog.FileName);
                    await new ContentDialog
                    {
                        Title = "导出祈愿记录完成",
                        Content = $"祈愿记录已导出至 {dialog.SafeFileName}",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            UnInitialize();
            base.OnNavigatingFrom(e);
        }
        private void UnInitialize()
        {
            this.Log("uninitialized");
            Service = null;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000);
            Service = new GachaStatisticService();
            DataContext = Service;
            Service.Initialize();
        }
    }
}
