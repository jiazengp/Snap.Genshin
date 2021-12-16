using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Controls.TitleBarButtons;
using DGP.Genshin.DataModel.Cookies;
using DGP.Genshin.Messages;
using DGP.Genshin.MiHoYoAPI.UserInfo;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
    public class UserInfoViewModel : ObservableObject, IRecipient<CookieAddedMeaasge>, IRecipient<CookieRemovedMessage>
    {
        private readonly ICookieService cookieService;

        private CookieUserInfo? selectedCookieUserInfo;
        private ObservableCollection<CookieUserInfo> cookieUserInfos = new();
        private IRelayCommand<TitleBarButton> initializeCommand;
        private IAsyncRelayCommand removeUserCommand;
        private IAsyncRelayCommand addUserCommand;
        private IAsyncRelayCommand loadCommand;

        public CookieUserInfo? SelectedCookieUserInfo
        {
            get => selectedCookieUserInfo; set
            {
                SetProperty(ref selectedCookieUserInfo, value);
                cookieService.ChangeOrIgnoreCurrentCookie(value?.Cookie);
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
        public IAsyncRelayCommand LoadCommand 
        { 
            get => loadCommand;
            [MemberNotNull(nameof(loadCommand))]
            set => SetProperty(ref loadCommand, value);
        }
        public IAsyncRelayCommand RemoveUserCommand
        {
            get => removeUserCommand;
            [MemberNotNull(nameof(removeUserCommand))]
            set => SetProperty(ref removeUserCommand, value);
        }
        public IAsyncRelayCommand AddUserCommand
        {
            get => addUserCommand;
            [MemberNotNull(nameof(addUserCommand))]
            set => SetProperty(ref addUserCommand, value);
        }

        public UserInfoViewModel(ICookieService cookieService)
        {
            this.cookieService = cookieService;
            LoadCommand = new AsyncRelayCommand(InitializeInternalAsync);
            InitializeCommand = new RelayCommand<TitleBarButton>(Initialize);
            RemoveUserCommand = new AsyncRelayCommand(RemoveUserAsync);
            AddUserCommand = new AsyncRelayCommand(AddUserAsync);
        }

        private async Task AddUserAsync()
        {
            await cookieService.AddNewCookieToPoolAsync();
        }

        private async Task RemoveUserAsync()
        {
            //this.HideAttachedFlyout();
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

        private void Initialize(TitleBarButton? t)
        {
            t?.ShowAttachedFlyout<System.Windows.Controls.Grid>(this);
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

        public async void Receive(CookieAddedMeaasge message)
        {
            string newCookie = message.Value;
            if ((await new UserInfoProvider(newCookie).GetUserInfoAsync()) is UserInfo newInfo)
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
