using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    [ViewModel(ViewModelType.Transient)]
    public class DailyNoteViewModel : ObservableObject
    {
        private bool isRefreshing;
        private List<DailyNote> dailyNotes = new();
        private IAsyncRelayCommand<TitleBarButton> refreshCommand;
        public List<DailyNote> DailyNotes { get => dailyNotes; set => SetProperty(ref dailyNotes, value); }
        public IAsyncRelayCommand<TitleBarButton> RefreshCommand { get => refreshCommand; set => refreshCommand = value; }
        public DailyNoteViewModel()
        {
            refreshCommand = new AsyncRelayCommand<TitleBarButton>(async (t) =>
            {
                if (t?.ShowAttachedFlyout<Grid>(this) == true)
                {
                    await RefreshAsync();
                }
            });
        }
        public async Task RefreshAsync()
        {
            if (isRefreshing)
            {
                return;
            }
            isRefreshing = true;
            List<DailyNote> list = new();
            UserGameRoleInfo? roles = await new UserGameRoleProvider(CookieManager.CurrentCookie).GetUserGameRolesAsync();

            if (roles?.List is not null)
            {
                foreach (UserGameRole role in roles.List)
                {
                    if (role.Region is not null && role.GameUid is not null)
                    {
                        DailyNote? note = await new DailyNoteProvider(CookieManager.CurrentCookie).GetDailyNoteAsync(role.Region, role.GameUid);
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
