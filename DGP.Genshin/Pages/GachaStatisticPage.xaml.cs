using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Services.GachaStatistic;
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

        private async void AutoFindAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (Service is not null)
            {
                await Service.RefreshAsync(GachaLogUrlMode.GameLogFile);
            }
        }

        private async void ManualInputUrlAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (Service is not null)
            {
                await Service.RefreshAsync(GachaLogUrlMode.ManualInput);
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
