using DGP.Genshin.Controls.TitleBarButtons;
using DGP.Genshin.DataModels.Cookies;
using DGP.Genshin.Helpers;
using DGP.Genshin.Messages;
using DGP.Genshin.MiHoYoAPI.UserInfo;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModels.TitleBarButtons
{
    [ViewModel(InjectAs.Transient)]
    public class UserInfoViewModel : ObservableRecipient2, IRecipient<CookieAddedMessage>, IRecipient<CookieRemovedMessage>
    {
        private readonly ICookieService cookieService;
        private UserInfoTitleBarButton? View;

        private CookieUserInfo? selectedCookieUserInfo;
        private ObservableCollection<CookieUserInfo> cookieUserInfos = new();

        public CookieUserInfo? SelectedCookieUserInfo
        {
            get => selectedCookieUserInfo;
            set => SetPropertyAndCallbackOnCompletion(ref selectedCookieUserInfo, value, v => cookieService.ChangeOrIgnoreCurrentCookie(v?.Cookie));
        }
        public ObservableCollection<CookieUserInfo> CookieUserInfos
        {
            get => cookieUserInfos;
            set => SetProperty(ref cookieUserInfos, value);
        }

        public ICommand OpenUICommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand RemoveUserCommand { get; }
        public ICommand AddUserCommand { get; }

        public UserInfoViewModel(ICookieService cookieService, IMessenger messenger) : base(messenger)
        {
            this.cookieService = cookieService;

            LoadCommand = new AsyncRelayCommand(OpenUIInternalAsync);
            OpenUICommand = new RelayCommand<TitleBarButton>(OpenUI);
            RemoveUserCommand = new AsyncRelayCommand(RemoveUserAsync);
            AddUserCommand = new AsyncRelayCommand(AddUserAsync);
        }

        private async Task AddUserAsync()
        {
            View?.HideAttachedFlyout();
            await cookieService.AddCookieToPoolOrIgnoreAsync();
        }
        private async Task RemoveUserAsync()
        {
            View?.HideAttachedFlyout();

            cookieService.CookiesLock.EnterWriteLock();

            if (cookieService.Cookies.Count <= 1)
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

            cookieService.CookiesLock.ExitWriteLock();

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
                    cookieService.Cookies.Remove(SelectedCookieUserInfo.Cookie);
                }
            }
        }
        private void OpenUI(TitleBarButton? t)
        {
            if (t?.ShowAttachedFlyout<System.Windows.Controls.Grid>(this) == true)
            {
                View = t as UserInfoTitleBarButton;
                new Event(t.GetType(), true).TrackAs(Event.OpenTitle);
            }
        }
        internal async Task OpenUIInternalAsync()
        {
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

        public async void Receive(CookieAddedMessage message)
        {
            string newCookie = message.Value;
            if (await new UserInfoProvider(newCookie).GetUserInfoAsync() is UserInfo newInfo)
            {
                CookieUserInfos.Add(new CookieUserInfo(newCookie, newInfo));
            }
        }
        public void Receive(CookieRemovedMessage message)
        {
            CookieUserInfo? prevSelected = SelectedCookieUserInfo;
            CookieUserInfo? currentRemoved = CookieUserInfos.First(u => u.Cookie == message.Value);
            CookieUserInfos.Remove(currentRemoved);
            if (prevSelected == currentRemoved)
            {
                SelectedCookieUserInfo = CookieUserInfos.First();
            }
        }
    }
}
