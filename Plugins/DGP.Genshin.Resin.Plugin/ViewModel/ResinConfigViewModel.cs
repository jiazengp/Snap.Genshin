using DGP.Genshin.DataModel.Cookie;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.Resin.Plugin.DataModel;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.Threading;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.Resin.Plugin.ViewModel
{
    [ViewModel(InjectAs.Singleton)]
    public class ResinConfigViewModel : ObservableRecipient2,
        IRecipient<CookieAddedMessage>,
        IRecipient<CookieRemovedMessage>,
        IRecipient<AppExitingMessage>
    {
        public static readonly SettingDefinition<List<ResinWidgetConfigration>?> ResinWidgetConfigrations =
            new(
                nameof(ResinWidgetConfigrations),
                null,
                Setting2.ComplexConverter<List<ResinWidgetConfigration>>);
        private readonly ICookieService cookieService;

        private ObservableCollection<ResinWidgetConfigration>? resinWidget;

        public ObservableCollection<ResinWidgetConfigration>? ResinWidgets
        {
            get => resinWidget;
            set => SetProperty(ref resinWidget, value);
        }

        public ResinConfigViewModel(ICookieService cookieService, IMessenger messenger) : base(messenger)
        {
            this.cookieService = cookieService;

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            List<CookieUserGameRole> cookieUserGameRoles = new();

            using (await cookieService.CookiesLock.WriteLockAsync())
            {
                foreach (string cookie in cookieService.Cookies)
                {
                    cookieUserGameRoles.AddRange(await cookieService.GetCookieUserGameRolesOfAsync(cookie));
                }
            }

            //首先初始化可用的列表
            ResinWidgets = new(cookieUserGameRoles
                .Select(role => new ResinWidgetConfigration { CookieUserGameRole = role }));
            //读取储存的状态
            List<ResinWidgetConfigration>? storedResinWidgets = ResinWidgetConfigrations.Get();
            //开始恢复状态
            if (storedResinWidgets?.Count > 0)
            {
                foreach (ResinWidgetConfigration widget in ResinWidgets)
                {
                    ResinWidgetConfigration? matched = storedResinWidgets
                        .FirstOrDefault(stored => 
                        stored.CookieUserGameRole?.Equals(widget.CookieUserGameRole) == true);
                    if (matched != null)
                    {
                        widget.IsPresent = matched.IsPresent;
                        widget.Top = matched.Top;
                        widget.Left = matched.Left;
                    }
                }
            }
            App.Current.Dispatcher.Invoke(() =>
            {
                //initialize widgets state
                foreach (ResinWidgetConfigration widget in ResinWidgets)
                {
                    widget.Initialize();
                }
            });
        }

        public void Receive(CookieAddedMessage message)
        {
            AddResinWidgetConfigurationsAsync(message).Forget();
        }

        private async Task AddResinWidgetConfigurationsAsync(CookieAddedMessage message)
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
                ResinWidgetConfigrations.Set(ResinWidgets);
                this.Log("resin widgets saved");
            }
            else
            {
                this.Log("no resin widget saved,collection is null");
            }
        }
    }
}