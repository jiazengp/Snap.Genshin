using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Request;
using DGP.Genshin.Models.MiHoYo.Sign;
using DGP.Genshin.Models.MiHoYo.User;
using DGP.Genshin.Services.Settings;
using DGP.Snap.Framework.Extensions.System;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// 每日签到服务
    /// </summary>
    public class DailySignInService
    {
        private const string ApiTakumi = @"https://api-takumi.mihoyo.com";
        private const string ReferBaseUrl = @"https://webstatic.mihoyo.com/bbs/event/signin-ys/index.html";
        private const string ActivityId = "e202009291139501";

        private static readonly string Referer =
            $"{ReferBaseUrl}?bbs_auth_required=true&act_id={ActivityId}&utm_source=bbs&utm_medium=mys&utm_campaign=icon";

        public DailySignInService()
        {
            this.Log("initialized");
        }

        public async Task<SignInInfo?> GetSignInInfoAsync(UserGameRole? role)
        {
            if (role is null)
            {
                return null;
            }
            string? cookie = CookieManager.Cookie;
            if (cookie is null)
            {
                return null;
            }

            Requester requester = new Requester(new RequestOptions
            {
                {"Accept", RequestOptions.Json },
                {"User-Agent",RequestOptions.CommonUA2_10_1 },
                {"x-rpc-device_id", RequestOptions.DeviceId },
                {"Referer", Referer },
                {"Cookie", cookie },
                {"X-Requested-With", RequestOptions.Hyperion }
            });

            return await Task.Run(() =>
            requester.Get<SignInInfo>($"{ApiTakumi}/event/bbs_sign_reward/info?act_id={ActivityId}&region={role.Region}&uid={role.GameUid}")?.Data);
        }
        public async Task<ReSignInfo?> GetReSignInfoAsync(UserGameRole role)
        {
            if (role is null)
            {
                return null;
            }
            string? cookie = CookieManager.Cookie;
            if(cookie is null)
            {
                return null;
            }

            Requester requester = new Requester(new RequestOptions
            {
                {"Accept", RequestOptions.Json },
                {"User-Agent",RequestOptions.CommonUA2_10_1 },
                {"Referer", Referer },
                {"Cookie", cookie },
                {"X-Requested-With", RequestOptions.Hyperion }
            });

            return await Task.Run(() =>
            requester.Get<ReSignInfo>($"{ApiTakumi}/event/bbs_sign_reward/resign_info?uid={role.GameUid}&region={role.Region}&act_id={ActivityId}")?.Data);
        }

        [SuppressMessage("", "IDE0050")]
        public async Task<SignInResult?> SignInAsync(UserGameRole? role)
        {
            if (role is null)
            {
                return null;
            }
            string? cookie = CookieManager.Cookie;
            if(cookie is null)
            {
                return null;
            }

            var data = new { act_id = ActivityId, region = role.Region, uid = role.GameUid };
            SettingService.Instance[Setting.LastAutoSignInTime] = DateTime.Now;

            Requester requester = new Requester(new RequestOptions
            {
                {"DS", DynamicSecretProvider.Create() },
                {"x-rpc-app_version", DynamicSecretProvider.AppVersion },
                {"User-Agent", RequestOptions.CommonUA2_10_1 },
                {"x-rpc-device_id", RequestOptions.DeviceId },
                {"Accept", RequestOptions.Json },
                {"x-rpc-client_type", "5" },
                {"Referer", Referer },
                {"Cookie", cookie },
                {"X-Requested-With", RequestOptions.Hyperion }
            });

            return await Task.Run(() =>
            requester.Post<SignInResult>($"{ApiTakumi}/event/bbs_sign_reward/sign", data)?.Data);
        }

        [SuppressMessage("", "IDE0050")]
        public async Task<SignInResult?> ReSignAsync(UserGameRole? role)
        {
            if (role is null)
            {
                return null;
            }
            string? cookie = CookieManager.Cookie;
            if(cookie is null)
            {
                return null;
            }

            Requester requester = new Requester(new RequestOptions
            {
                {"DS", DynamicSecretProvider.Create() },
                {"x-rpc-app_version", DynamicSecretProvider.AppVersion },
                {"User-Agent", RequestOptions.CommonUA2_10_1 },
                {"x-rpc-device_id", RequestOptions.DeviceId },
                {"Accept", RequestOptions.Json },
                {"x-rpc-client_type", "5" },
                {"Referer", Referer },
                {"Cookie", cookie },
                {"X-Requested-With", RequestOptions.Hyperion }
            });

            var data = new { act_id = ActivityId, region = role.Region, uid = role.GameUid };
            return await Task.Run(() =>
            requester.Post<SignInResult>($"{ApiTakumi}/event/bbs_sign_reward/sign", data)?.Data);
        }

        public async Task<UserGameRoleInfo?> GetUserGameRolesAsync()
        {
            string? cookie = CookieManager.Cookie;
            if(cookie is null)
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

            return await Task.Run(() =>
            requester.Get<UserGameRoleInfo>($"{ApiTakumi}/binding/api/getUserGameRolesByCookie?game_biz=hk4e_cn")?.Data);
        }
        public async Task<SignInReward?> GetSignInRewardAsync()
        {
            string? cookie = CookieManager.Cookie;
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

            return await Task.Run(() =>
            requester.Get<SignInReward>($"{ApiTakumi}/event/bbs_sign_reward/home?act_id={ActivityId}")?.Data);
        }
    }
}
