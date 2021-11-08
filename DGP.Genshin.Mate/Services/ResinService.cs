using DGP.Genshin.Common.Data.Behavior;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DGP.Genshin.Mate.Services
{
    public class ResinService : Observable
    {
        private List<UserGameRoleDailyNote>? dailyNotes;
        public List<UserGameRoleDailyNote>? DailyNotes { get => dailyNotes; set => Set(ref dailyNotes, value); }

        private bool isRefreshing;
        public bool IsRefreshing { get => isRefreshing; set => Set(ref isRefreshing, value); }

        public async Task RefreshAsync()
        {
            if (IsRefreshing)
            {
                return;
            }
            IsRefreshing = true;
            List<UserGameRoleDailyNote> list = new();
            foreach (string cookie in CookieManager.Cookies)
            {
                UserGameRoleInfo? roles = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();

                if (roles?.List is not null)
                {
                    foreach (UserGameRole role in roles.List)
                    {
                        if (role.Region is not null && role.GameUid is not null)
                        {
                            DailyNote? note = await new DailyNoteProvider(cookie).GetDailyNoteAsync(role.Region, role.GameUid);
                            if (note is not null)
                            {
                                list.Add(new(role, note));
                            }
                        }
                    }
                }
            }
            DailyNotes = list;
            IsRefreshing = false;
        }

        public class UserGameRoleDailyNote
        {
            public UserGameRoleDailyNote(UserGameRole role, DailyNote note)
            {
                UserGameRole = role;
                DailyNote = note;
            }
            public UserGameRole? UserGameRole { get; set; }
            public DailyNote? DailyNote { get; set; }
        }

        #region 单例
        private static volatile ResinService? instance;
        [SuppressMessage("", "IDE0044")]
        private static object _locker = new();
        private ResinService() { }
        public static ResinService Instance
        {
            get
            {
                if (instance is null)
                {
                    lock (_locker)
                    {
                        instance ??= new();
                    }
                }
                return instance;
            }
        }
        #endregion
    }



}
