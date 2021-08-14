using DGP.Genshin.Services;
using System;
using System.ComponentModel;
using System.Windows;

namespace DGP.Genshin
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        private bool isLoggedIn;
        private readonly LoginService service;
        public LoginWindow(LoginService service)
        {
            this.service = service;
            service.LoginWindow = this;
            this.InitializeComponent();
        }

        private void LoginWebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            this.Activate();
            if (e.Uri != null && e.Uri.OriginalString == "https://user.mihoyo.com/#/account/home")
            {
                this.isLoggedIn = true;
                this.service.OnLogin(true);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!this.isLoggedIn)
            {
                this.service.OnLogin(false);
            }
        }

        private void Window_Closed(object sender, EventArgs e) => this.LoginWebBrowser.Dispose();
    }
}
