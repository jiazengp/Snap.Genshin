using DGP.Snap.Framework.Extensions.System;
using ModernWpf.Controls;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DGP.Genshin.Models.MiHoYo
{
    /// <summary>
    /// CookieDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CookieDialog : ContentDialog
    {
        public CookieDialog()
        {
            InitializeComponent();
            this.Log("initialized");
        }

        public async Task<string> GetInputCookieAsync()
        {
            await ShowAsync();
            return this.InputText.Text;
        }

        private void AutoCookieButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            using (CookieWindow cookieWindow = new CookieWindow())
            {
                cookieWindow.ShowDialog();
                bool isLoggedIn = cookieWindow.IsLoggedIn;
                if (isLoggedIn)
                {
                    this.InputText.Text = cookieWindow.Cookie;
                }
            }
        }

        private void InputText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
            this.IsPrimaryButtonEnabled = this.InputText.Text.Contains("account_id");
    }
}
