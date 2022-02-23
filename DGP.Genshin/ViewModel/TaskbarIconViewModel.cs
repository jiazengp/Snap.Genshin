using DGP.Genshin.DataModel.Cookie;
using DGP.Genshin.DataModel.DailyNote;
using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class TaskbarIconViewModel : ObservableRecipient2,
        IRecipient<CookieAddedMessage>,
        IRecipient<CookieRemovedMessage>,
        IRecipient<AppExitingMessage>
    {
        private readonly ICookieService cookieService;

        private ObservableCollection<ResinWidgetConfigration>? resinWidget;

        public ObservableCollection<ResinWidgetConfigration>? ResinWidgets
        {
            get => resinWidget;
            set => SetProperty(ref resinWidget, value);
        }

        public ICommand ShowMainWindowCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand UpdateWidgetsCommand { get; }
        public ICommand RestartElevatedCommand { get; }

        public TaskbarIconViewModel(ICookieService cookieService, IScheduleService scheduleService, IMessenger messenger)
            : base(messenger)
        {
            this.cookieService = cookieService;

            scheduleService.Initialize();
            InitializeResinWidgetsAsync();

            ShowMainWindowCommand = new RelayCommand(OpenMainWindow);
            ExitCommand = new RelayCommand(ExitApp);
            UpdateWidgetsCommand = new RelayCommand(UpdateWidgets);
            RestartElevatedCommand = new RelayCommand(RestartElevated);
        }

        private async void InitializeResinWidgetsAsync()
        {
            List<CookieUserGameRole> cookieUserGameRoles = new();

            cookieService.CookiesLock.EnterReadLock();
            foreach (string cookie in cookieService.Cookies)
            {
                cookieUserGameRoles.AddRange(await cookieService.GetCookieUserGameRolesOf(cookie));
            }
            cookieService.CookiesLock.ExitReadLock();

            //首先初始化可用的列表
            ResinWidgets = new(cookieUserGameRoles.Select(role => new ResinWidgetConfigration { CookieUserGameRole = role }));
            //读取储存的状态
            List<ResinWidgetConfigration>? storedResinWidgets = Setting2.ResinWidgetConfigrations.Get();
            //开始恢复状态
            if (storedResinWidgets?.Count > 0)
            {
                foreach (ResinWidgetConfigration widget in ResinWidgets)
                {
                    ResinWidgetConfigration? matched = storedResinWidgets
                        .FirstOrDefault(stored => stored.CookieUserGameRole?.Equals(widget.CookieUserGameRole) == true);
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
            App.Current.Dispatcher.Invoke(() => App.BringWindowToFront<MainWindow>());
        }
        private void ExitApp()
        {
            App.Current.Shutdown();
        }
        private void UpdateWidgets()
        {
            App.Messenger.Send<TickScheduledMessage>();
        }
        private void RestartElevated()
        {
            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    FileName = PathContext.Locate("DGP.Genshin.exe"),
                });
            }
            catch (Win32Exception)
            {
                return;
            }
            
            App.Current.Shutdown();
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
            IEnumerable<ResinWidgetConfigration> targets = ResinWidgets!
                .Where(u => u.CookieUserGameRole!.Cookie == message.Value);
            //.ToList() fix exception with enumrate a modified collection.
            foreach (ResinWidgetConfigration target in targets.ToList())
            {
                target.IsChecked = false;
                ResinWidgets?.Remove(target);
            }
        }
        public void Receive(AppExitingMessage message)
        {
            if (ResinWidgets is not null)
            {
                foreach (ResinWidgetConfigration? widget in ResinWidgets)
                {
                    widget.UpdatePropertyState();
                }
                Setting2.ResinWidgetConfigrations.Set(ResinWidgets);
            }
            else
            {
                this.Log("no resin widget saved,collection is null");
            }
        }
    }
}
