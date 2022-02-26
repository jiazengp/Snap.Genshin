using DGP.Genshin.DataModel.Cookie;
using DGP.Genshin.DataModel.DailyNote;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;
using DGP.Genshin.MiHoYoAPI.UserInfo;
using DGP.Genshin.Service.Abstraction;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Extenion.Enumerable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    public class UserViewModel : ObservableRecipient2,
        IRecipient<CookieAddedMessage>,
        IRecipient<CookieRemovedMessage>,
        IRecipient<DailyNotesRefreshedMessage>
    {
        private readonly ICookieService cookieService;
        private readonly IDailyNoteService dailynoteService;

        private ObservableCollection<CookieUserInfo> cookieUserInfos = new();
        private CookieUserInfo? selectedCookieUserInfo;
        private List<CookieUserGameRole>? cookieUserGameRoles;
        private CookieUserGameRole? selectedCookieGameRole;
        private DailyNote? dailyNote;
        private DailyNoteNotifyConfiguration dailyNoteNotifyConfiguration;

        public ObservableCollection<CookieUserInfo> CookieUserInfos
        {
            get => cookieUserInfos;
            set => SetProperty(ref cookieUserInfos, value);
        }
        public CookieUserInfo? SelectedCookieUserInfo
        {
            get => selectedCookieUserInfo;
            set => SetPropertyAndCallbackOnCompletion(ref selectedCookieUserInfo, value, OnSelectedCookieUserInfoChanged);
        }
        [PropertyChangedCallback]
        private async void OnSelectedCookieUserInfoChanged(CookieUserInfo? cookieUserInfo)
        {
            if (cookieUserInfo != null)
            {
                cookieService.ChangeOrIgnoreCurrentCookie(cookieUserInfo.Cookie);
                //update user game roles
                List<UserGameRole> userGameRoles = await new UserGameRoleProvider(cookieService.CurrentCookie).GetUserGameRolesAsync();
                CookieUserGameRoles = userGameRoles
                    .Select(role => new CookieUserGameRole(cookieUserInfo.Cookie, role))
                    .ToList();
                SelectedCookieGameRole = CookieUserGameRoles.MatchedOrFirst(i => i.UserGameRole.IsChosen);
            }
        }

        public List<CookieUserGameRole>? CookieUserGameRoles { get => cookieUserGameRoles; set => SetProperty(ref cookieUserGameRoles, value); }
        public CookieUserGameRole? SelectedCookieGameRole
        {
            get => selectedCookieGameRole;
            set => SetPropertyAndCallbackOnCompletion(ref selectedCookieGameRole, value, OnSelectedCookieGameRoleChanged);
        }
        [PropertyChangedCallback]
        public void OnSelectedCookieGameRoleChanged(CookieUserGameRole? cookieUserGameRole)
        {
            UpdateDailyNote(cookieUserGameRole);
        }

        private void UpdateDailyNote(CookieUserGameRole? cookieUserGameRole)
        {
            if (cookieUserGameRole != null)
            {
                DailyNote = dailynoteService.GetDailyNote(cookieUserGameRole);
            }
        }

        public DailyNote? DailyNote { get => dailyNote; set => SetProperty(ref dailyNote, value); }
        public DailyNoteNotifyConfiguration DailyNoteNotifyConfiguration
        {
            get => dailyNoteNotifyConfiguration;
            [MemberNotNull(nameof(dailyNoteNotifyConfiguration))]
            set => SetProperty(ref dailyNoteNotifyConfiguration, value);
        }

        public ICommand OpenUICommand { get; }
        public ICommand RemoveUserCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand RefreshDailyNoteCommand { get; }

        public UserViewModel(ICookieService cookieService, IDailyNoteService dailyNoteService, IMessenger messenger) : base(messenger)
        {
            this.cookieService = cookieService;
            this.dailynoteService = dailyNoteService;

            //与设置项同步
            DailyNoteNotifyConfiguration = Setting2.DailyNoteNotifyConfiguration.Get() ?? new();
            Setting2.DailyNoteNotifyConfiguration.Set(DailyNoteNotifyConfiguration);

            OpenUICommand = new AsyncRelayCommand(OpenUIAsync);
            RemoveUserCommand = new AsyncRelayCommand(RemoveUserAsync);
            AddUserCommand = new AsyncRelayCommand(AddUserAsync);
            RefreshDailyNoteCommand = new RelayCommand(RequestDailyNoteRefresh);
        }

        private async Task OpenUIAsync()
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
        private async Task AddUserAsync()
        {
            await cookieService.AddCookieToPoolOrIgnoreAsync();
        }
        private async Task RemoveUserAsync()
        {
            if (cookieService.Cookies.Count <= 1)
            {
                await App.Current.Dispatcher.InvokeAsync(new ContentDialog()
                {
                    Title = "删除用户失败",
                    Content = "我们需要至少一个用户的信息才能使程序正常运转。",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync).Task.Unwrap();
                //fix remove cookie crash.
                return;
            }

            if (SelectedCookieUserInfo is not null)
            {
                ContentDialogResult result = await App.Current.Dispatcher.InvokeAsync(new ContentDialog()
                {
                    Title = "确定要删除该用户吗?",
                    Content = "删除用户的操作不可撤销。",
                    PrimaryButtonText = "确定",
                    SecondaryButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync).Task.Unwrap();

                if (result is ContentDialogResult.Primary)
                {
                    cookieService.Cookies.Remove(SelectedCookieUserInfo.Cookie);
                }
            }
        }
        private void RequestDailyNoteRefresh()
        {
            App.Messenger.Send(new TickScheduledMessage());
        }

        public async void Receive(CookieAddedMessage message)
        {
            try
            {
                string newCookie = message.Value;
                this.Log("new Cookie added");
                if (await new UserInfoProvider(newCookie).GetUserInfoAsync() is UserInfo newInfo)
                {
                    CookieUserInfos.Add(new CookieUserInfo(newCookie, newInfo));
                }
                this.Log(cookieUserInfos.Count);
            }
            catch (Exception ex)
            {
                this.Log(ex);
                Crashes.TrackError(ex);
            }
        }
        public void Receive(CookieRemovedMessage message)
        {
            this.Log("Cookie removed");
            CookieUserInfo? prevSelected = SelectedCookieUserInfo;
            CookieUserInfo? currentRemoved = CookieUserInfos.First(u => u.Cookie == message.Value);
            CookieUserInfos.Remove(currentRemoved);
            if (prevSelected == currentRemoved)
            {
                SelectedCookieUserInfo = CookieUserInfos.First();
            }
            this.Log(cookieUserInfos.Count);
        }
        public void Receive(DailyNotesRefreshedMessage message)
        {
            this.Log("daily note updated");
            UpdateDailyNote(SelectedCookieGameRole);
        }
    }
}
