using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Request;
using DGP.Genshin.Models.MiHoYo.Sign;
using DGP.Genshin.Models.MiHoYo.User;
using DGP.Snap.Framework.Extensions.System;
using System.Collections.Generic;
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

        public async Task<List<Result>> SignInAsync()
        {
            List<Result> results = new List<Result>();
            string cookie = await CookieManager.GetCookieAsync();
            Response<UserGameRoleInfo> rolesInfo = new Requester(new RequestOptions
            {
                {"Accept", RequestOptions.Json },
                {"User-Agent", RequestOptions.CommonUA },
                {"Referer", Referer },
                {"Cookie", cookie },
                {"X-Requested-With", RequestOptions.Hyperion }
            }).Get<UserGameRoleInfo>($"{ApiTakumi}/binding/api/getUserGameRolesByCookie?game_biz=hk4e_cn");
            results.Add(rolesInfo);

            if (rolesInfo.ReturnCode == 0)
            {
                int accountBindCount = rolesInfo.Data.List.Count;
                for (int i = 0; i < accountBindCount; i++)
                {
                    UserGameRole role = rolesInfo.Data.List[i];
                    Response<SignIn> signIn = new Requester(new RequestOptions
                    {
                        {"Accept", RequestOptions.Json },
                        {"User-Agent",RequestOptions.CommonUA },
                        {"x-rpc-device_id", RequestOptions.DeviceId },
                        {"Referer", Referer },
                        {"Cookie", cookie },
                        {"X-Requested-With", RequestOptions.Hyperion }
                    }).Get<SignIn>($"{ApiTakumi}/event/bbs_sign_reward/info?act_id={ActivityId}&region={role.Region}&uid={role.GameUid}");
                    results.Add(signIn);
                    if (signIn.ReturnCode == 0)
                    {
                        //don't ever convert to value tuple,just ignore the intellisense
                        var data = new { act_id = ActivityId, region = role.Region, uid = role.GameUid };

                        Response<SignInResult> result = new Requester(new RequestOptions
                        {
                            {"DS", DynamicSecretProvider.Create() },
                            {"x-rpc-app_version", DynamicSecretProvider.AppVersion/*"2.2.1"*/ },
                            {"User-Agent", RequestOptions.CommonUA },
                            {"x-rpc-device_id", RequestOptions.DeviceId },
                            {"Accept", RequestOptions.Json },
                            {"x-rpc-client_type", "5" },
                            {"Referer", Referer },
                            {"Cookie", cookie },
                            {"X-Requested-With", RequestOptions.Hyperion }
                        }).Post<SignInResult>($"{ApiTakumi}/event/bbs_sign_reward/sign", data);
                        results.Add(result);
                    }
                }
            }
            return results;
        }
    }
}
