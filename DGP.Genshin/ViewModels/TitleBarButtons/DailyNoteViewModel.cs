using DGP.Genshin.Controls.TitleBarButtons;
using DGP.Genshin.Helpers;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using ModernWpf.Controls.Primitives;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Extenion.Enumerable;
using Snap.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DGP.Genshin.ViewModels.TitleBarButtons
{
    [ViewModel(InjectAs.Transient)]
    public class DailyNoteViewModel : ObservableObject
    {
        private readonly ICookieService cookieService;
        private readonly TaskPreventer taskPreventer = new();

        private List<DailyNote> dailyNotes = new();

        public List<DailyNote> DailyNotes { get => dailyNotes; set => SetProperty(ref dailyNotes, value); }

        public ICommand OpenUICommand { get; }

        public DailyNoteViewModel(ICookieService cookieService, IMessenger messenger)
        {
            this.cookieService = cookieService;
            OpenUICommand = new AsyncRelayCommand<TitleBarButton>(OpenUIAsync);
        }

        private async Task OpenUIAsync(TitleBarButton? t)
        {
            if (t?.ShowAttachedFlyout<Grid>(this) == true)
            {
                new Event(t.GetType(), true).TrackAs(Event.OpenTitle);
                await RefreshDailyNotesAsync();
            }
        }
        private async Task RefreshDailyNotesAsync()
        {
            if (taskPreventer.ShouldExecute)
            {
                List<DailyNote> list = new();
                List<UserGameRole> roles = await new UserGameRoleProvider(cookieService.CurrentCookie).GetUserGameRolesAsync();

                foreach (UserGameRole role in roles)
                {
                    if (role.Region is not null && role.GameUid is not null)
                    {
                        DailyNote? note = await new DailyNoteProvider(cookieService.CurrentCookie).GetDailyNoteAsync(role.Region, role.GameUid);
                        bool result = list.AddIfNotNull(note);
                        this.Log($"add dailynote to result :{result}");
                    }
                }
                DailyNotes = list;
                taskPreventer.Release();
            }
        }
    }
}
