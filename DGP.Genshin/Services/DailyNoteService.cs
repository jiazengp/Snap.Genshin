using DGP.Genshin.Cookie;
using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Record.DailyNote;
using DGP.Genshin.Models.MiHoYo.Request;
using DGP.Genshin.Models.MiHoYo.User;
using DGP.Snap.Framework.Data.Behavior;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    public class DailyNoteService : Observable
    {
        private const string ApiTakumi = @"https://api-takumi.mihoyo.com";
        private const string BaseUrl = @"https://api-takumi.mihoyo.com/game_record/app/genshin/api";
        private const string Referer = @"https://webstatic.mihoyo.com/app/community-game-records/index.html?v=6";

        private List<DailyNote>? dailyNotes;
        public List<DailyNote>? DailyNotes { get => dailyNotes; set => Set(ref dailyNotes, value); }

        private bool isRefreshing = false;
        public async Task RefreshAsync()
        {
            if (isRefreshing)
            {
                return;
            }
            isRefreshing = true;
            List<DailyNote> list = new List<DailyNote>();
            UserGameRoleInfo? roles = await GetUserGameRolesAsync();
            if (roles is not null && roles.List is not null)
            {
                foreach (UserGameRole role in roles.List)
                {
                    DailyNote? note = GetDailyNote(role.Region, role.GameUid);
                    if (note is not null)
                    {
                        list.Add(note);
                    }
                }
                DailyNotes = list;
            }
            isRefreshing = false;
        }

        public async Task<UserGameRoleInfo?> GetUserGameRolesAsync()
        {
            string? cookie = CookieManager.Cookies;
            if (cookie is null)
            {
                return null;
            }

            Requester requester = new Requester(new RequestOptions
            {
                {"Accept", RequestOptions.Json },
                {"User-Agent", RequestOptions.CommonUA2_10_1 },
                {"Referer", Referer },
                {"Cookie", cookie },
                {"X-Requested-With", RequestOptions.Hyperion }
            });

            return await Task.Run(() => requester.Get<UserGameRoleInfo>($"{ApiTakumi}/binding/api/getUserGameRolesByCookie?game_biz=hk4e_cn")?.Data);
        }

        public DailyNote? GetDailyNote(string? server, string? uid)
        {
            string? cookie = CookieManager.Cookies;
            if (cookie is null || server is null || uid is null)
            {
                return null;
            }

            Requester requester = new Requester(new RequestOptions
            {
                {"Accept", RequestOptions.Json },
                {"x-rpc-app_version", DynamicSecretProvider2.AppVersion },
                {"User-Agent", RequestOptions.CommonUA2_11_1 },
                {"x-rpc-client_type", "5" },
                {"Referer",Referer },
                {"Cookie", cookie },
                {"X-Requested-With", RequestOptions.Hyperion }
            });

            return TryGet($"{BaseUrl}/dailyNote?server={server}&role_id={uid}", requester, out Response<DailyNote>? resp)
                ? resp?.Data
                : null;
        }

        private bool TryGet<T>(string url, Requester requester, out Response<T>? response)
        {
            requester.Headers["DS"] = DynamicSecretProvider2.Create(url);
            response = requester.Get<T>(url);
            return response?.ReturnCode == 0;
        }
    }
}
