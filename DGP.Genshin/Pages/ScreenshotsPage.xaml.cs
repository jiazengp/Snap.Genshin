using DGP.Genshin.Services.Screenshots;
using System.IO;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// ScreenshotsPage.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenshotsPage : Page
    {
        public ScreenshotsPage()
        {
            this.DataContext = ScreenshotService.Instance;
            ScreenshotService.Instance.Initialize();
            InitializeComponent();
        }

        private void RemoveScreenshotClick(object sender, System.Windows.RoutedEventArgs e)
        {
            string path = (string)((MenuItem)sender).Tag;
            File.Delete(path);
        }
    }
}
