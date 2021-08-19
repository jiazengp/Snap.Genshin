using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Sign;
using DGP.Genshin.Models.MiHoYo.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    public class DailySignInService
    {
        private const string BaseUrl = "https://api-takumi.mihoyo.com/";
        private const string ActId = "e202009291139501";

        public async Task<List<Result>> SignInAsync()
        {
            List<Result> results = new List<Result>();
            string cookie = await CookieManager.GetCookieAsync();

            Requester requester = new Requester(cookie);

            Response<UserGameRoleInfo> rolesInfo =
                requester.Get<UserGameRoleInfo>($"{BaseUrl}binding/api/getUserGameRolesByCookie?game_biz=hk4e_cn");
            results.Add(rolesInfo);
            if (rolesInfo.ReturnCode == 0)
            {
                int accountBindCount = rolesInfo.Data.List.Count;

                for (int i = 0; i < accountBindCount; i++)
                {
                    UserGameRole role = rolesInfo.Data.List[i];
                    Response<SignIn> signIn =
                        requester.Get<SignIn>($"{BaseUrl}event/bbs_sign_reward/info?act_id={ActId}&region={role.Region}&uid={role.GameUid}", true);
                    results.Add(signIn);
                    if (signIn.ReturnCode == 0)
                    {
                        var data = new { act_id = ActId, region = role.Region, uid = role.GameUid };

                        Response<SignInResult> result =
                            requester.Post<SignInResult>($"{BaseUrl}event/bbs_sign_reward/sign", data, true);
                        results.Add(result);

                    }
                }
            }
            return results;
        }
    }
}
