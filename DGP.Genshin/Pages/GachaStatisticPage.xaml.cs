using DGP.Genshin.Services.GachaStatistic;
using Microsoft.Win32;
using ModernWpf.Controls;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// GachaStatisticPage.xaml 的交互逻辑
    /// </summary>
    public partial class GachaStatisticPage : Page
    {
        private GachaStatisticService Service { get; set; }

        public GachaStatisticPage()
        {
            this.Service = new GachaStatisticService();
            this.DataContext = this.Service;
            InitializeComponent();
        }
        private void RefreshAppBarButtonClick(object sender, RoutedEventArgs e) =>
            this.Service.Refresh();
        private async void ExportExcelAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Excel 工作簿|*.xlsx",
                Title = "保存到表格",
                ValidateNames = true,
                CheckPathExists = true
            };
            if (dialog.ShowDialog() == true)
            {
                await this.Service.ExportDataToExcelAsync(dialog.FileName);
                await new ContentDialog
                {
                    Title = "导出祈愿记录完成",
                    Content = $"祈愿记录已导出至 {dialog.SafeFileName}",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
        }
        private async void ExportImageAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "PNG 图像|*.png",
                Title = "导出至图片",
                ValidateNames = true,
                CheckPathExists = true
            };
            if (dialog.ShowDialog() == true)
            {
                this.TitleGrid.Visibility = Visibility.Visible;
                double offset = this.ContentScrollViewer.CurrentVerticalOffset;
                this.ContentScrollViewer.ScrollToTop();
                this.ContentScrollViewer.UpdateLayout();
                Matrix dpiMatrix = PresentationSource.FromDependencyObject(this.Container).CompositionTarget.TransformToDevice;
                RenderTargetBitmap bitmap = new RenderTargetBitmap(
                    (int)this.Container.ActualWidth,
                    (int)this.Container.ActualHeight,
                    dpiMatrix.OffsetX,
                    dpiMatrix.OffsetY,
                    PixelFormats.Pbgra32);
                bitmap.Render(this.Container);
                this.ContentScrollViewer.ScrollToVerticalOffset(offset);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using (FileStream fs = File.Create(dialog.FileName))
                {
                    encoder.Save(fs);
                }
                this.TitleGrid.Visibility = Visibility.Collapsed;
                await new ContentDialog
                {
                    Title = "导出图片完成",
                    Content = $"图片已导出至 {dialog.SafeFileName}",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            UnInitialize();
            base.OnNavigatingFrom(e);
        }
        private void UnInitialize()
        {
            this.Service.UnInitialize();
            this.Service = null;
        }

        private async void ImportFromGenshinGachaExportAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JS对象简谱文件|*.json",
                Title = "从 Genshin Gacha Export 记录文件导入",
                Multiselect = false,
                CheckFileExists = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                await Service.ImportFromGenshinGachaExportAsync(openFileDialog.FileName);
            }
        }
    }
}
