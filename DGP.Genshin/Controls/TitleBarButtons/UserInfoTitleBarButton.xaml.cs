using DGP.Genshin.DataModel.Cookies;
using DGP.Genshin.MiHoYoAPI.UserInfo;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    /// <summary>
    /// UserInfoTitleBarButton.xaml 的交互逻辑
    /// </summary>
    public partial class UserInfoTitleBarButton : TitleBarButton
    {
        public UserInfoTitleBarButton()
        {
            //suppress the databinding warning
            //PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
            InitializeComponent();
            DataContext = this;
        }

        private async void RemoveCookieAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            this.HideAttachedFlyout();
            if (CookieService.Cookies.Count <= 1)
            {
                await App.Current.Dispatcher.InvokeAsync(new ContentDialog()
                {
                    Title = "删除用户失败",
                    Content = "我们需要至少一个用户的信息才能使程序正常运转。",
                    PrimaryButtonText = "确定"
                }.ShowAsync).Task.Unwrap();
                //fix remove cookie crash.
                return;
            }

            if (SelectedCookieUserInfo is not null)
            {
                ContentDialogResult result = await App.Current.Dispatcher.InvokeAsync(new ContentDialog()
                {
                    Title = "确定要删除该用户吗?",
                    Content = "删除用户操作不可撤销。",
                    PrimaryButtonText = "确定",
                    SecondaryButtonText = "取消"
                }.ShowAsync).Task.Unwrap();
                if (result is ContentDialogResult.Primary)
                {
                    CookieService.Cookies.Remove(SelectedCookieUserInfo.Cookie);
                }
            }
        }

        private async void AddCookieAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            this.HideAttachedFlyout();
            await CookieService.AddNewCookieToPoolAsync();
        }

    }
    public class UserInfoViewModel : ObservableObject
    {
        private readonly ICookieService cookieService;

        private CookieUserInfo? selectedCookieUserInfo;
        private ObservableCollection<CookieUserInfo> cookieUserInfos = new();
        private IRelayCommand<TitleBarButton> initializeCommand;

        public CookieUserInfo? SelectedCookieUserInfo
        {
            get => selectedCookieUserInfo; set
            {
                SetProperty(ref selectedCookieUserInfo, value);
                CookieService.ChangeOrIgnoreCurrentCookie(value?.Cookie);
            }
        }
        public ObservableCollection<CookieUserInfo> CookieUserInfos
        {
            get => cookieUserInfos;
            set => SetProperty(ref cookieUserInfos, value);
        }
        public IRelayCommand<TitleBarButton> InitializeCommand 
        { 
            get => initializeCommand; 
            [MemberNotNull(nameof(initializeCommand))]
            set => SetProperty(ref initializeCommand, value); 
        }
        public IAsyncRelayCommand RemoveUserCommand { get; set; }
        public UserInfoViewModel(ICookieService cookieService)
        {
            this.cookieService = cookieService;
            cookieService.Cookies.CookieAdded += OnCookiesAdded;
            cookieService.Cookies.CookieRemoved += OnCookieRemoved;
            InitializeCommand = new RelayCommand<TitleBarButton>(InitializeAsync);
        }

        private void InitializeAsync(TitleBarButton? t)
        {
            t?.ShowAttachedFlyout<System.Windows.Controls.Grid>(this);
        }

        private async void OnCookiesAdded(string newCookie)
        {
            UserInfo? newInfo = await new UserInfoProvider(newCookie).GetUserInfoAsync();
            if (newInfo is not null)
            {
                CookieUserInfos.Add(new CookieUserInfo(newCookie, newInfo));
            }
        }
        public void OnCookieRemoved(string cookie)
        {
            CookieUserInfo? prevSelected = SelectedCookieUserInfo;
            CookieUserInfo? currentRemoved = CookieUserInfos.First(u => u.Cookie == cookie);
            CookieUserInfos.Remove(currentRemoved);
            if (prevSelected == currentRemoved)
            {
                SelectedCookieUserInfo = CookieUserInfos.First();
            }
        }

        internal async Task InitializeInternalAsync()
        {
            CookieUserInfos.Clear();
            foreach (string cookie in cookieService.Cookies)
            {
                UserInfo? info = await new UserInfoProvider(cookie).GetUserInfoAsync();
                if (info is not null)
                {
                    CookieUserInfos.Add(new CookieUserInfo(cookie, info));
                }
            }
            SelectedCookieUserInfo = CookieUserInfos.FirstOrDefault(c => c.Cookie == cookieService.CurrentCookie);
        }
    }
}
