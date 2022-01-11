using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Helpers;
using ModernWpf.Controls;
using System.Threading.Tasks;

namespace DGP.Genshin.Controls.Cookies
{
    public partial class CookieDialog : ContentDialog
    {
        public CookieDialog()
        {
            InitializeComponent();
            this.Log("initialized");
        }

        /// <summary>
        /// 获取输入的Cookie
        /// </summary>
        public async Task<(ContentDialogResult result, string cookie)> GetInputCookieAsync()
        {
            ContentDialogResult result = await ShowAsync();
            return (result, InputText.Text);
        }

        private void AutoCookieButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!WebView2Helper.IsSupported)
            {
                System.Windows.Controls.Button button = (System.Windows.Controls.Button)sender;
                button.IsEnabled = false;
                button.Content = "需要先安装WebView2运行时";
            }
            else
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
        }

        private void InputText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string text = InputText.Text;
            bool inputEmpty = string.IsNullOrEmpty(text);
            bool inputHasAccountId = text.Contains("account_id");

            (PrimaryButtonText, IsPrimaryButtonEnabled) = (inputEmpty, inputHasAccountId) switch
            {
                (true, _) => ("请输入Cookie", false),
                (false, true) => ("确认", true),
                (false, false) => ("该Cookie无效", false)
            };
        }
    }
}
