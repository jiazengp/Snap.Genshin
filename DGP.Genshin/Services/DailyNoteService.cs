using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Record.DailyNote;
using DGP.Genshin.Models.MiHoYo.Request;
using DGP.Genshin.Models.MiHoYo.UserInfo;
using DGP.Snap.Framework.Data.Behavior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    public class DailyNoteService : Observable
    {
        private const string BaseUrl = @"https://api-takumi.mihoyo.com/game_record/app/genshin/api";
        private const string Referer = @"https://webstatic.mihoyo.com/app/community-game-records/index.html?v=6";

        private List<DailyNote> dailyNotes;
        public List<DailyNote> DailyNotes { get => dailyNotes; set => Set(ref dailyNotes, value); }

        public void RefreshAsync()
        {

        }

        public DailyNote GetDailyNote(string server, string uid)
        {
            string cookie = CookieManager.Cookie;
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

            return TryGet($"{BaseUrl}/dailyNote?server={server}&role_id={uid}", requester, out Response<DailyNote> resp)
                ? resp.Data
                : null;
        }

        private bool TryGet<T>(string url, Requester requester, out Response<T> response)
        {
            requester.Headers["DS"] = DynamicSecretProvider2.Create(url);
            response = requester.Get<T>(url);
            return response.ReturnCode == 0;
        }
    }
}
