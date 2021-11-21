using DGP.Genshin.Common.Extensions.System;
using ModernWpf.Controls;
using System.Threading.Tasks;

namespace DGP.Genshin.Cookie
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns>string.Empty if user force return</returns>
        public async Task<string> GetInputCookieAsync()
        {
            await ShowAsync();
            return InputText.Text.Contains("account_id") ? InputText.Text : string.Empty;
        }

        private void AutoCookieButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            using (CookieWindow cookieWindow = new())
            {
                cookieWindow.ShowDialog();
                bool isLoggedIn = cookieWindow.IsLoggedIn;
                if (isLoggedIn)
                {
                    InputText.Text = cookieWindow.Cookie;
                }
            }
        }

        private void InputText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            IsPrimaryButtonEnabled = InputText.Text.Contains("account_id");
        }
    }
}
