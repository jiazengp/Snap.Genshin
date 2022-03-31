using ModernWpf.Controls;
using Snap.Data.Primitive;
using Snap.Win32;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.Control.Cookie
{
    /// <summary>
    /// Cookie对话框
    /// </summary>
    public sealed partial class CookieDialog : ContentDialog
    {
        private const string CookieCode = "javascript:(()=>{_=(n)=>{for(i in(r=document.cookie.split(';'))){var a=r[i].split('=');if(a[0].trim()==n)return a[1]}};c=_('account_id')||alert('无效的Cookie,请重新登录!');c&&confirm('将Cookie复制到剪贴板?')&&copy(document.cookie)})();";

        /// <summary>
        /// 构造一个新的对话框实例
        /// </summary>
        public CookieDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 获取输入的Cookie
        /// </summary>
        /// <returns>输入的结果</returns>
        public async Task<Result<bool, string>> GetInputCookieAsync()
        {
            ContentDialogResult result = await ShowAsync();
            string cookie = InputText.Text;

            return new(result != ContentDialogResult.Secondary, cookie);
        }

        private void AutoCookieButtonClick(object sender, RoutedEventArgs e)
        {
            if (!WebView2Helper.IsSupported)
            {
                Button button = (Button)sender;
                button.IsEnabled = false;
                button.Content = "需要先安装 WebView2运行时";

                new WebView2RuntimeWindow().ShowDialog();
            }
            else
            {
                using (CookieWindow cookieWindow = new())
                {
                    cookieWindow.ShowDialog();
                    if (cookieWindow.IsLoggedIn)
                    {
                        InputText.Text = cookieWindow.Cookie;
                    }
                }
            }
        }

        private void InputTextChanged(object sender, TextChangedEventArgs e)
        {
            string text = InputText.Text;

            bool inputEmpty = string.IsNullOrEmpty(text);
            bool inputHasAccountId = text.Contains("account_id");

            (PrimaryButtonText, IsPrimaryButtonEnabled) = (inputEmpty, inputHasAccountId) switch
            {
                (true, _) => ("请输入Cookie", false),
                (false, true) => ("确认", true),
                (false, false) => ("该Cookie无效", false),
            };
        }

        private void CopyCodeButtonClick(object sender, RoutedEventArgs e)
        {
            // clear before copy
            Clipboard.Clear();
            try
            {
                Clipboard.SetText(CookieCode);
            }
            catch
            {
                try
                {
                    Clipboard2.SetText(CookieCode);
                }
                catch
                {
                }
            }
        }
    }
}