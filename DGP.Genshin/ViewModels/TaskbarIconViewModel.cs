using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.DataModels.Cookies;
using DGP.Genshin.DataModels.DailyNotes;
using DGP.Genshin.Messages;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
    internal class TaskbarIconViewModel : ObservableRecipient, IRecipient<CookieAddedMessage>, IRecipient<CookieRemovedMessage>
    {
        private readonly ICookieService cookieService;
        private readonly ISettingService settingService;

        private ICommand showMainWindowCommand;
        private ICommand exitCommand;
        private ObservableCollection<ResinWidgetConfigration>? resinWidget;
        private ICommand updateWidgetsCommand;

        public ICommand ShowMainWindowCommand
        {
            get => showMainWindowCommand;
            [MemberNotNull(nameof(showMainWindowCommand))]
            set => SetProperty(ref showMainWindowCommand, value);
        }
        public ICommand ExitCommand
        {
            get => exitCommand;
            [MemberNotNull(nameof(exitCommand))]
            set => SetProperty(ref exitCommand, value);
        }
        public ICommand UpdateWidgetsCommand
        {
            get => updateWidgetsCommand;
            [MemberNotNull(nameof(updateWidgetsCommand))]
            set => SetProperty(ref updateWidgetsCommand, value);
        }
        public ObservableCollection<ResinWidgetConfigration>? ResinWidgets
        {
            get => resinWidget;
            set => SetProperty(ref resinWidget, value);
        }

        public TaskbarIconViewModel(ICookieService cookieService, IScheduleService scheduleService, ISettingService settingService, IMessenger messenger)
            : base(messenger)
        {
            this.cookieService = cookieService;
            this.settingService = settingService;

            scheduleService.Initialize();
            InitializeResinWidgetsAsync();

            ShowMainWindowCommand = new RelayCommand(OpenMainWindow);
            ExitCommand = new RelayCommand(ExitApp);
            UpdateWidgetsCommand = new RelayCommand(UpdateWidgets);

            IsActive = true;
        }
        ~TaskbarIconViewModel()
        {
            IsActive = false;
        }

        private async void InitializeResinWidgetsAsync()
        {
            List<CookieUserGameRole> cookieUserGameRoles = new();

            cookieService.CookiesLock.EnterReadLock();
            foreach (string cookie in cookieService.Cookies)
            {
                List<UserGameRole> userGameRoles = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();
                cookieUserGameRoles.AddRange(userGameRoles.Select(u => new CookieUserGameRole(cookie, u)));
            }
            cookieService.CookiesLock.ExitReadLock();

            //首先初始化可用的列表
            ResinWidgets = new(cookieUserGameRoles.Select(role => new ResinWidgetConfigration { CookieUserGameRole = role }));
            //读取储存的状态
            List<ResinWidgetConfigration>? storedResinWidgets =
                settingService.GetComplexOrDefault<List<ResinWidgetConfigration>>(Setting.ResinWidgetConfigrations, null);
            //开始恢复状态
            if (storedResinWidgets?.Count > 0)
            {
                foreach (ResinWidgetConfigration widget in ResinWidgets)
                {
                    ResinWidgetConfigration? matched = storedResinWidgets.FirstOrDefault(stored => stored.CookieUserGameRole == widget.CookieUserGameRole);
                    if (matched != null)
                    {
                        widget.IsPresent = matched.IsPresent;
                        widget.Top = matched.Top;
                        widget.Left = matched.Left;
                    }
                }
            }
            //initialize widgets state
            foreach (ResinWidgetConfigration widget in ResinWidgets)
            {
                widget.Initialize();
            }
        }
        private void OpenMainWindow()
        {
            App.ShowOrCloseWindow<MainWindow>();
        }
        private void ExitApp()
        {
            SaveResinWidgetConfigrations();
            App.Current.Shutdown();
        }
        private void SaveResinWidgetConfigrations()
        {
            if (ResinWidgets is not null)
            {
                foreach (ResinWidgetConfigration? widget in ResinWidgets)
                {
                    widget.UpdatePropertyState();
                }
                settingService[Setting.ResinWidgetConfigrations] = ResinWidgets;
            }
        }
        private void UpdateWidgets()
        {
            App.Messenger.Send<TickScheduledMessage>();
        }
        public async void Receive(CookieAddedMessage message)
        {
            string cookie = message.Value;
            List<CookieUserGameRole> results = new();
            List<UserGameRole> userGameRoles = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();
            results.AddRange(userGameRoles.Select(u => new CookieUserGameRole(cookie, u)));
            foreach (CookieUserGameRole role in results)
            {
                ResinWidgets?.Add(new ResinWidgetConfigration { CookieUserGameRole = role });
            }
        }
        public void Receive(CookieRemovedMessage message)
        {
            IEnumerable<ResinWidgetConfigration> targets = ResinWidgets!.Where(u => u.CookieUserGameRole!.Cookie == message.Value);

            foreach (ResinWidgetConfigration target in targets)
            {
                target.IsChecked = false;
                ResinWidgets?.Remove(target);
            }
        }
    }
}
