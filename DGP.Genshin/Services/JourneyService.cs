using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Journey;
using DGP.Genshin.Models.MiHoYo.Request;
using DGP.Genshin.Models.MiHoYo.User;
using DGP.Snap.Framework.Data.Behavior;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// 旅行札记服务
    /// </summary>
    public class JourneyService : Observable
    {
        private const string ApiTakumi = @"https://api-takumi.mihoyo.com";
        private const string ApiHk4e = "https://hk4e-api.mihoyo.com";
        private const string ReferBaseUrl = @"https://webstatic.mihoyo.com/bbs/event/e20200709ysjournal/index.html";
        private const string BBSStyle = "bbs_presentation_style=fullscreen&bbs_auth_required=true&utm_source=bbs&utm_medium=mys&utm_campaign=icon";

        private static readonly string Referer = $"{ReferBaseUrl}?{BBSStyle}";

        public JourneyService()
        {
            UserGameRoleChanged += OnUserGameRoleChanged;
        }

        #region API Interop
        /// <summary>
        /// 获取月份信息
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="region"></param>
        /// <param name="month">0为起始请求</param>
        /// <returns></returns>
        public async Task<JourneyInfo> GetMonthInfoAsync(string uid, string region, int month = 0)
        {
            if (uid == null || region == null)
            {
                return null;
            }
            string cookie = CookieManager.Cookie;
            Requester requester = new Requester(new RequestOptions
            {
                {"User-Agent", RequestOptions.CommonUA2_10_1 },
                {"Referer",Referer },
                {"Cookie", cookie },
                {"X-Requested-With", RequestOptions.Hyperion }
            });
            Response<JourneyInfo> resp = await requester.GetAsync<JourneyInfo>
                ($@"{ApiHk4e}/event/ys_ledger/monthInfo?month={month}&bind_uid={uid}&bind_region={region}&{BBSStyle}");
            return resp.Data;
        }

        /// <summary>
        /// 一次请求10条记录
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="region"></param>
        /// <param name="month"></param>
        /// <param name="type">1：原石，2：摩拉</param>
        /// <param name="page">请求的页码</param>
        /// <returns>当返回列表的数量不足10个时应停止请求</returns>
        public async Task<JourneyDetail> GetMonthDetailAsync(string uid, string region, int month, int type, int page = 1)
        {
            string cookie = CookieManager.Cookie;
            Requester requester = new Requester(new RequestOptions
            {
                {"User-Agent", RequestOptions.CommonUA2_10_1 },
                {"Referer",Referer },
                {"Cookie", cookie },
                {"X-Requested-With", RequestOptions.Hyperion }
            });
            Response<JourneyDetail> resp = await requester.GetAsync<JourneyDetail>
                ($@"{ApiHk4e}/event/ys_ledger/monthDetail?page={page}&month={month}&limit=10&type=2&bind_uid={uid}&bind_region={region}&{BBSStyle}");
            return resp.Data;
        }

        /// <summary>
        /// 获取用户角色信息
        /// </summary>
        /// <returns>用户角色信息</returns>
        public async Task<UserGameRoleInfo> GetUserGameRolesAsync()
        {
            string cookie = CookieManager.Cookie;
            Requester requester = new Requester(new RequestOptions
            {
                {"Accept", RequestOptions.Json },
                {"User-Agent", RequestOptions.CommonUA2_10_1 },
                {"Referer", Referer },
                {"Cookie", cookie },
                {"X-Requested-With", RequestOptions.Hyperion }
            });
            Response<UserGameRoleInfo> resp = await requester.GetAsync<UserGameRoleInfo>
                ($"{ApiTakumi}/binding/api/getUserGameRolesByCookie?game_biz=hk4e_cn");
            return resp.Data;
        }
        #endregion

        #region Observable
        private JourneyInfo journeyInfo;
        private UserGameRoleInfo userGameRoleInfo;
        private UserGameRole selectedRole;

        public JourneyInfo JourneyInfo { get => this.journeyInfo; set => Set(ref this.journeyInfo, value); }
        public UserGameRoleInfo UserGameRoleInfo { get => this.userGameRoleInfo; set => Set(ref this.userGameRoleInfo, value); }
        public UserGameRole SelectedRole
        {
            get => this.selectedRole; set
            {
                Set(ref this.selectedRole, value);
                UserGameRoleChanged?.Invoke(value);
            }
        }
        #endregion

        public async Task InitializeAsync()
        {
            this.UserGameRoleInfo = await GetUserGameRolesAsync();
            this.SelectedRole = this.UserGameRoleInfo?.List.First();
        }
        private async void OnUserGameRoleChanged(UserGameRole role) => this.JourneyInfo = await GetMonthInfoAsync(role?.GameUid, role?.Region);

        private event UserGameRoleChangedHandler UserGameRoleChanged;
    }
    public delegate void UserGameRoleChangedHandler(UserGameRole role);
}
