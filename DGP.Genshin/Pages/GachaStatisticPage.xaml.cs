using DGP.Genshin.Services.GachaStatistic;
using ModernWpf.Controls;
using System;
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
            this.InitializeComponent();
        }
        private void RefreshAppBarButtonClick(object sender, System.Windows.RoutedEventArgs e) => this.Service.Refresh();
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Service.UnInitialize();
            base.OnNavigatedFrom(e);
        }
        private async void ExportExcelAppBarButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Service.ExportDataToExcelAsync();
            await new ContentDialog
            {
                Title = "导出祈愿记录完成",
                Content = $"请查看桌面上的 {this.Service.SelectedUid}.xlsx 文件",
                PrimaryButtonText = "确定",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
        }
        private async void ExportImageButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            TitleGrid.Visibility = Visibility.Visible;
            Container.UpdateLayout();
            Matrix dpiMatrix = PresentationSource.FromDependencyObject(Container).CompositionTarget.TransformToDevice;
            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                (int)Container.ActualWidth, 
                (int)Container.ActualHeight, 
                dpiMatrix.OffsetX, 
                dpiMatrix.OffsetY, 
                PixelFormats.Pbgra32);
            bitmap.Render(Container);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            path = $@"{path}\{Service.SelectedUid}.png";
            using (FileStream fs = File.Create(path))
            {
                encoder.Save(fs);
            }
            TitleGrid.Visibility = Visibility.Collapsed;
            await new ContentDialog
            {
                Title = "导出图片完成",
                Content = $"请查看桌面上的 {this.Service.SelectedUid}.png 文件",
                PrimaryButtonText = "确定",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
            
        }
    }
}
