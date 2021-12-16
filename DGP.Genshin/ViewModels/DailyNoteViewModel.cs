using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Controls.TitleBarButtons;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
    public class DailyNoteViewModel : ObservableObject
    {
        private readonly ICookieService cookieService;

        private List<DailyNote> dailyNotes = new();
        private IAsyncRelayCommand<TitleBarButton> refreshCommand;

        public List<DailyNote> DailyNotes { get => dailyNotes; set => SetProperty(ref dailyNotes, value); }
        public IAsyncRelayCommand<TitleBarButton> RefreshCommand
        {
            get => refreshCommand;
            [MemberNotNull(nameof(refreshCommand))]
            set => refreshCommand = value;
        }

        public DailyNoteViewModel(ICookieService cookieService)
        {
            this.cookieService = cookieService;
            RefreshCommand = new AsyncRelayCommand<TitleBarButton>(RefreshAsync);
        }
        private async Task RefreshAsync(TitleBarButton? t)
        {
            if (t?.ShowAttachedFlyout<Grid>(this) == true)
            {
                await RefreshInternalAsync();
            }
        }

        private bool isRefreshing;
        private async Task RefreshInternalAsync()
        {
            if (isRefreshing)
            {
                return;
            }
            isRefreshing = true;
            List<DailyNote> list = new();
            UserGameRoleInfo? roles = await new UserGameRoleProvider(cookieService.CurrentCookie).GetUserGameRolesAsync();

            if (roles?.List is not null)
            {
                foreach (UserGameRole role in roles.List)
                {
                    if (role.Region is not null && role.GameUid is not null)
                    {
                        DailyNote? note = await new DailyNoteProvider(cookieService.CurrentCookie).GetDailyNoteAsync(role.Region, role.GameUid);
                        if (note is not null)
                        {
                            list.Add(note);
                        }
                    }
                }
            }
            DailyNotes = list;
            isRefreshing = false;
        }
    }
}
