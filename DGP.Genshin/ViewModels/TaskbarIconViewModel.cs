using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Data.Behavior;
using DGP.Genshin.Controls.GenshinElements;
using DGP.Genshin.DataModels.Behavior;
using DGP.Genshin.DataModels.Cookies;
using DGP.Genshin.Messages;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
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

        public class DailyNoteCheckable : Checkable<Pair<CookieUserGameRole, DailyNoteWindow?>>
        {
            public DailyNoteCheckable(Pair<CookieUserGameRole, DailyNoteWindow?> value,
                Action<bool, Pair<CookieUserGameRole, DailyNoteWindow?>> checkChangedCallback)
                : base(value, checkChangedCallback) { }
        }

        private ICommand showMainWindowCommand;
        private ICommand exitCommand;
        private ObservableCollection<DailyNoteCheckable>? userGameRoles;

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
        public ObservableCollection<DailyNoteCheckable>? UserGameRoles
        {
            get => userGameRoles;
            set => SetProperty(ref userGameRoles, value);
        }

        public TaskbarIconViewModel(ICookieService cookieService, IScheduleService scheduleService,IMessenger messenger) : base(messenger)
        {
            this.cookieService = cookieService;

            scheduleService.Initialize();
            InitializeUserGameRolesAsync();

            ShowMainWindowCommand = new RelayCommand(OpenMainWindow);
            ExitCommand = new RelayCommand(ExitApp);
        }
        private async void InitializeUserGameRolesAsync()
        {
            List<CookieUserGameRole> results = new();
            foreach (string cookie in cookieService.Cookies)
            {
                List<UserGameRole> userGameRoles = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();
                results.AddRange(userGameRoles.Select(u => new CookieUserGameRole(cookie, u)));
            }
            UserGameRoles = new(results.Select(u => new DailyNoteCheckable(new(u, null), OnCheckChanged)));
        }
        private void OnCheckChanged(bool isChecked, Pair<CookieUserGameRole, DailyNoteWindow?> roleWindowPair)
        {
            if (isChecked)
            {
                roleWindowPair.Value = new DailyNoteWindow(new DailyNoteResinViewModel(roleWindowPair.Key, cookieService, App.Messenger));
                roleWindowPair.Value.Show();
            }
            else
            {
                roleWindowPair.Value?.Close();
                roleWindowPair.Value = null;
            }
        }

        private void OpenMainWindow()
        {
            App.ShowOrCloseWindow<MainWindow>();
        }
        private void ExitApp()
        {
            App.Current.Shutdown();
        }

        public async void Receive(CookieAddedMessage message)
        {
            string cookie = message.Value;
            List<CookieUserGameRole> results = new();
            List<UserGameRole> userGameRoles = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();
            results.AddRange(userGameRoles.Select(u => new CookieUserGameRole(cookie, u)));
            foreach(CookieUserGameRole role in results)
            {
                UserGameRoles?.Add(new DailyNoteCheckable(new(role, null), OnCheckChanged));
            }
        }
        public void Receive(CookieRemovedMessage message)
        {
            DailyNoteCheckable? target = UserGameRoles?.FirstOrDefault(u => u.Value.Key.Cookie == message.Value);
            if(target is not null)
            {
                target.Value.Value?.Close();
                target.Value.Value = null;
                UserGameRoles?.Remove(target);
            }
        }
    }
}
