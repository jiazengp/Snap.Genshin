using DGP.Genshin.Control.Infrastructure.Concurrent;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class UserViewModel : ObservableRecipient2, ISupportCancellation,
        IRecipient<CookieAddedMessage>,
        IRecipient<CookieRemovedMessage>,
        IRecipient<DailyNotesRefreshedMessage>
    {
        private readonly ICookieService cookieService;
        private readonly IDailyNoteService dailynoteService;
        private readonly IMessenger messenger;

        public CancellationToken CancellationToken { get; set; }

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
            get => this.autoDailySignInOnLaunch;

            set
            {
                Setting2.AutoDailySignInOnLaunch.Set(value, false);
                this.SetProperty(ref this.autoDailySignInOnLaunch, value);
            }
        }
        public bool SignInSilently
        {
            get => this.signInSilently;

            set
            {
                Setting2.SignInSilently.Set(value, false);
                this.SetProperty(ref this.signInSilently, value);
            }
        }

        public ICommand SignInImmediatelyCommand { get; }
        #endregion

        #region Observable
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
            get => this.selectedResinAutoRefreshTime;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedResinAutoRefreshTime, value, v => Setting2.ResinRefreshMinutes.Set(v.Value.TotalMinutes));
        }
        public ObservableCollection<CookieUserInfo> CookieUserInfos
        {
            get => this.cookieUserInfos;

            set => this.SetProperty(ref this.cookieUserInfos, value);
        }
        public CookieUserInfo? SelectedCookieUserInfo
        {
            get => this.selectedCookieUserInfo;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedCookieUserInfo, value, this.OnSelectedCookieUserInfoChangedAsync);
        }
        [PropertyChangedCallback]
        private async Task OnSelectedCookieUserInfoChangedAsync(CookieUserInfo? cookieUserInfo)
        {
            try
            {
                if (cookieUserInfo != null)
                {
                    this.cookieService.ChangeOrIgnoreCurrentCookie(cookieUserInfo.Cookie);
                    //update user game roles
                    List<UserGameRole> userGameRoles = await new UserGameRoleProvider(this.cookieService.CurrentCookie).GetUserGameRolesAsync(this.CancellationToken);
                    this.CookieUserGameRoles = userGameRoles
                        .Select(role => new CookieUserGameRole(cookieUserInfo.Cookie, role))
                        .ToList();
                    this.SelectedCookieUserGameRole = this.CookieUserGameRoles.MatchedOrFirst(i => i.UserGameRole.IsChosen);
                }
            }
            catch (TaskCanceledException) { this.Log("OnSelectedCookieUserInfoChangedAsync canceled by user switch page"); }

        }

        public List<CookieUserGameRole>? CookieUserGameRoles
        {
            get => this.cookieUserGameRoles;

            set => this.SetProperty(ref this.cookieUserGameRoles, value);
        }
        public CookieUserGameRole? SelectedCookieUserGameRole
        {
            get => this.selectedCookieUserGameRole;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedCookieUserGameRole, value, this.OnSelectedCookieUserGameRoleChangedAsync);
        }
        [PropertyChangedCallback]
        public async Task OnSelectedCookieUserGameRoleChangedAsync(CookieUserGameRole? cookieUserGameRole)
        {
            this.UpdateDailyNote(cookieUserGameRole);
            if (cookieUserGameRole is not null)
            {
                UserGameRole role = cookieUserGameRole.UserGameRole;
                this.JourneyInfo = await new JourneyProvider(cookieUserGameRole.Cookie).GetMonthInfoAsync(role);
            }
        }

        private void UpdateDailyNote(CookieUserGameRole? cookieUserGameRole)
        {
            if (cookieUserGameRole != null)
            {
                this.DailyNote = this.dailynoteService.GetDailyNote(cookieUserGameRole);
            }
        }

        public DailyNote? DailyNote
        {
            get => this.dailyNote;

            set => this.SetProperty(ref this.dailyNote, value);
        }
        public DailyNoteNotifyConfiguration DailyNoteNotifyConfiguration
        {
            get => this.dailyNoteNotifyConfiguration;

            [MemberNotNull(nameof(dailyNoteNotifyConfiguration))]
            set => this.SetProperty(ref this.dailyNoteNotifyConfiguration, value);
        }
        public JourneyInfo? JourneyInfo
        {
            get => this.journeyInfo;

            set => this.SetProperty(ref this.journeyInfo, value);
        }
        #endregion

        public ICommand OpenUICommand { get; }
        public ICommand RemoveUserCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand RefreshCommand { get; }

        public UserViewModel(ICookieService cookieService, IDailyNoteService dailyNoteService, ISignInService signInService, IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger) : base(messenger)
        {
            this.cookieService = cookieService;
            this.dailynoteService = dailyNoteService;
            this.messenger = messenger;

            //实时树脂
            this.DailyNoteNotifyConfiguration = Setting2.DailyNoteNotifyConfiguration.GetNonValueType(() => new());
            this.selectedResinAutoRefreshTime = this.ResinAutoRefreshTimes.First(s => s.Value.TotalMinutes == Setting2.ResinRefreshMinutes);
            //签到选项
            this.AutoDailySignInOnLaunch = Setting2.AutoDailySignInOnLaunch;
            this.SignInSilently = Setting2.SignInSilently.Get();
            this.SignInImmediatelyCommand = asyncRelayCommandFactory.Create(signInService.TrySignAllAccountsRolesInAsync);

            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
            this.RemoveUserCommand = asyncRelayCommandFactory.Create(this.RemoveUserAsync);
            this.AddUserCommand = asyncRelayCommandFactory.Create(this.AddUserAsync);
            this.RefreshCommand = new RelayCommand(this.RefreshUI);
        }

        private async Task OpenUIAsync()
        {
            try
            {
                foreach (string cookie in this.cookieService.Cookies)
                {
                    if (await new UserInfoProvider(cookie).GetUserInfoAsync(this.CancellationToken) is UserInfo info)
                    {
                        this.CookieUserInfos.Add(new CookieUserInfo(cookie, info));
                    }
                }
                this.SelectedCookieUserInfo = this.CookieUserInfos.FirstOrDefault(c => c.Cookie == this.cookieService.CurrentCookie);
            }
            catch (TaskCanceledException) { this.Log("Open UI canceled"); }
        }
        private async Task AddUserAsync()
        {
            await this.cookieService.AddCookieToPoolOrIgnoreAsync();
            this.RefreshUI();
        }
        private async Task RemoveUserAsync()
        {
            if (this.cookieService.Cookies.Count <= 1)
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

            if (this.SelectedCookieUserInfo is not null)
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
                    this.cookieService.Cookies.Remove(this.SelectedCookieUserInfo.Cookie);
                }
            }
        }
        private void RefreshUI()
        {
            this.messenger.Send(new UserRequestRefreshMessage());
        }

        public void Receive(CookieAddedMessage message)
        {
            string newCookie = message.Value;
            this.AddCookieUserInfoAsync(newCookie).Forget();
        }
        public void Receive(CookieRemovedMessage message)
        {
            this.RemoveCookieUserInfo(message);
        }
        public void Receive(DailyNotesRefreshedMessage message)
        {
            this.Log("daily note updated");
            this.UpdateDailyNote(this.SelectedCookieUserGameRole);
        }

        private async Task AddCookieUserInfoAsync(string newCookie)
        {
            try
            {
                this.Log("new Cookie added");
                if (await new UserInfoProvider(newCookie).GetUserInfoAsync() is UserInfo newInfo)
                {
                    //Can't use JoinableTaskFactory.SwitchToMainThreadAsync here
                    App.Current.Dispatcher.Invoke(() => this.CookieUserInfos.Add(new CookieUserInfo(newCookie, newInfo)));
                }

                this.Log(this.cookieUserInfos.Count);
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
            CookieUserInfo? prevSelected = this.SelectedCookieUserInfo;
            CookieUserInfo? currentRemoved = this.CookieUserInfos.First(u => u.Cookie == message.Value);

            this.CookieUserInfos.Remove(currentRemoved);

            if (prevSelected == currentRemoved)
            {
                this.SelectedCookieUserInfo = this.CookieUserInfos.First();
            }
            this.Log(this.cookieUserInfos.Count);
        }
    }
}
