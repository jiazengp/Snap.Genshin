using DGP.Genshin.DataModel.Cookie;
using DGP.Genshin.DataModel.DailyNote;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Journey;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;
using DGP.Genshin.MiHoYoAPI.UserInfo;
using DGP.Genshin.Service.Abstraction;
using DGP.Genshin.Service.Abstraction.Setting;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.Threading;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Data.Primitive;
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
        private readonly IMessenger messenger;

        private ObservableCollection<CookieUserInfo> cookieUserInfos = new();
        private CookieUserInfo? selectedCookieUserInfo;
        private List<CookieUserGameRole>? cookieUserGameRoles;
        private CookieUserGameRole? selectedCookieUserGameRole;
        private DailyNote? dailyNote;
        private DailyNoteNotifyConfiguration dailyNoteNotifyConfiguration;
        private JourneyInfo? journeyInfo;

        #region SignIn
        private bool autoDailySignInOnLaunch;
        private bool signInSilently;

        public bool AutoDailySignInOnLaunch
        {
            get => autoDailySignInOnLaunch;

            set
            {
                Setting2.AutoDailySignInOnLaunch.Set(value, false);
                SetProperty(ref autoDailySignInOnLaunch, value);
            }
        }
        public bool SignInSilently
        {
            get => signInSilently;

            set
            {
                Setting2.SignInSilently.Set(value, false);
                SetProperty(ref signInSilently, value);
            }
        }

        public ICommand SignInImmediatelyCommand { get; }
        #endregion

        public List<NamedValue<TimeSpan>> ResinAutoRefreshTimes { get; } = new()
        {
            new("4 分钟 | 0.5 树脂", TimeSpan.FromMinutes(4)),
            new("8 分钟 | 1 树脂", TimeSpan.FromMinutes(8)),
            new("30 分钟 | 3.75 树脂", TimeSpan.FromMinutes(30)),
            new("40 分钟 | 5 树脂", TimeSpan.FromMinutes(40)),
            new("1 小时 | 7.5 树脂", TimeSpan.FromMinutes(60))
        };
        private NamedValue<TimeSpan> selectedResinAutoRefreshTime;
        public NamedValue<TimeSpan> SelectedResinAutoRefreshTime
        {
            get => selectedResinAutoRefreshTime;

            set => SetPropertyAndCallbackOnCompletion(ref selectedResinAutoRefreshTime, value, v => Setting2.ResinRefreshMinutes.Set(v.Value.TotalMinutes));
        }
        public ObservableCollection<CookieUserInfo> CookieUserInfos
        {
            get => cookieUserInfos;

            set => SetProperty(ref cookieUserInfos, value);
        }
        public CookieUserInfo? SelectedCookieUserInfo
        {
            get => selectedCookieUserInfo;

            set => SetPropertyAndCallbackOnCompletion(ref selectedCookieUserInfo, value, OnSelectedCookieUserInfoChangedAsync);
        }
        [PropertyChangedCallback]
        private async Task OnSelectedCookieUserInfoChangedAsync(CookieUserInfo? cookieUserInfo)
        {
            if (cookieUserInfo != null)
            {
                cookieService.ChangeOrIgnoreCurrentCookie(cookieUserInfo.Cookie);
                //update user game roles
                List<UserGameRole> userGameRoles = await new UserGameRoleProvider(cookieService.CurrentCookie).GetUserGameRolesAsync();
                CookieUserGameRoles = userGameRoles
                    .Select(role => new CookieUserGameRole(cookieUserInfo.Cookie, role))
                    .ToList();
                SelectedCookieUserGameRole = CookieUserGameRoles.MatchedOrFirst(i => i.UserGameRole.IsChosen);
            }
        }

        public List<CookieUserGameRole>? CookieUserGameRoles
        {
            get => cookieUserGameRoles;

            set => SetProperty(ref cookieUserGameRoles, value);
        }
        public CookieUserGameRole? SelectedCookieUserGameRole
        {
            get => selectedCookieUserGameRole;

            set => SetPropertyAndCallbackOnCompletion(ref selectedCookieUserGameRole, value, OnSelectedCookieUserGameRoleChanged);
        }
        [PropertyChangedCallback]
        public void OnSelectedCookieUserGameRoleChanged(CookieUserGameRole? cookieUserGameRole)
        {
            UpdateDailyNote(cookieUserGameRole);
            UpdateJourneyInfoAsync(cookieUserGameRole).Forget();
        }

        private void UpdateDailyNote(CookieUserGameRole? cookieUserGameRole)
        {
            if (cookieUserGameRole != null)
            {
                DailyNote = dailynoteService.GetDailyNote(cookieUserGameRole);
            }
        }

        private async Task UpdateJourneyInfoAsync(CookieUserGameRole? cookieUserGameRole)
        {
            if (cookieUserGameRole is not null)
            {
                UserGameRole role = cookieUserGameRole.UserGameRole;
                JourneyInfo = await new JourneyProvider(cookieUserGameRole.Cookie).GetMonthInfoAsync(role);
            }
        }

        public DailyNote? DailyNote
        {
            get => dailyNote;

            set => SetProperty(ref dailyNote, value);
        }
        public DailyNoteNotifyConfiguration DailyNoteNotifyConfiguration
        {
            get => dailyNoteNotifyConfiguration;

            [MemberNotNull(nameof(dailyNoteNotifyConfiguration))]
            set => SetProperty(ref dailyNoteNotifyConfiguration, value);
        }
        public JourneyInfo? JourneyInfo
        {
            get => journeyInfo;

            set => SetProperty(ref journeyInfo, value);
        }

        public ICommand OpenUICommand { get; }
        public ICommand RemoveUserCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand RefreshCommand { get; }

        public UserViewModel(ICookieService cookieService, IDailyNoteService dailyNoteService, ISignInService signInService, IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger) : base(messenger)
        {
            this.cookieService = cookieService;
            dailynoteService = dailyNoteService;
            this.messenger = messenger;

            //实时树脂
            DailyNoteNotifyConfiguration = Setting2.DailyNoteNotifyConfiguration.GetNonValueType(() => new());
            selectedResinAutoRefreshTime = ResinAutoRefreshTimes.First(s => s.Value.TotalMinutes == Setting2.ResinRefreshMinutes);
            //签到选项
            AutoDailySignInOnLaunch = Setting2.AutoDailySignInOnLaunch;
            SignInSilently = Setting2.SignInSilently.Get();
            SignInImmediatelyCommand = asyncRelayCommandFactory.Create(signInService.TrySignAllAccountsRolesInAsync);

            OpenUICommand = asyncRelayCommandFactory.Create(OpenUIAsync);
            RemoveUserCommand = asyncRelayCommandFactory.Create(RemoveUserAsync);
            AddUserCommand = asyncRelayCommandFactory.Create(AddUserAsync);
            RefreshCommand = new RelayCommand(RefreshUI);
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
            RefreshUI();
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
        private void RefreshUI()
        {
            messenger.Send(new UserRequestRefreshMessage());
        }

        public void Receive(CookieAddedMessage message)
        {
            string newCookie = message.Value;
            AddCookieUserInfoAsync(newCookie).Forget();
        }
        public void Receive(CookieRemovedMessage message)
        {
            RemoveCookieUserInfo(message);
        }
        public void Receive(DailyNotesRefreshedMessage message)
        {
            this.Log("daily note updated");
            UpdateDailyNote(SelectedCookieUserGameRole);
        }

        private async Task AddCookieUserInfoAsync(string newCookie)
        {
            try
            {
                this.Log("new Cookie added");
                if (await new UserInfoProvider(newCookie).GetUserInfoAsync() is UserInfo newInfo)
                {
                    //Can't use JoinableTaskFactory.SwitchToMainThreadAsync here
                    App.Current.Dispatcher.Invoke(() => CookieUserInfos.Add(new CookieUserInfo(newCookie, newInfo)));
                }

                this.Log(cookieUserInfos.Count);
            }
            catch (Exception ex)
            {
                this.Log(ex);
                Crashes.TrackError(ex);
            }
        }
        private void RemoveCookieUserInfo(CookieRemovedMessage message)
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
    }
}
