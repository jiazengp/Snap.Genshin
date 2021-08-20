using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.BBSAPI;
using DGP.Genshin.Models.MiHoYo.BBSAPI.Post;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    internal class MiHoYoBBSService
    {
        public const string BaseUrl = @"https://bbs-api.mihoyo.com/user/wapi";
        public const string PostBaseUrl = @"https://bbs-api.mihoyo.com/post/wapi";
        public async Task<UserInfo> GetCurrentUserInfoAsync()
        {
            Requester requester = new Requester(await CookieManager.GetCookieAsync());
            Response<UserInfoWrapper> resp = await Task.Run(() =>
            requester.Get<UserInfoWrapper>($"{BaseUrl}/getUserFullInfo?gids=2"));
            return resp.ReturnCode == 0 ? resp.Data.UserInfo : null;
        }

        public async Task<List<Post>> GettOfficialRecommendedPostsAsync()
        {
            Requester requester = new Requester(await CookieManager.GetCookieAsync());
            Response<PostWrapper> resp = await Task.Run(() =>
            requester.Get<PostWrapper>($"{PostBaseUrl}/getOfficialRecommendedPosts?gids=2"));
            return resp.Data.List;
        }

        public async Task<dynamic> GetPostFullAsync(string postId)
        {
            //requester.referer need to be Referer: https://bbs.mihoyo.com/
            Requester requester = new Requester(await CookieManager.GetCookieAsync(), @"https://bbs.mihoyo.com/");
            Response<dynamic> resp = await Task.Run(() =>
            requester.Get<dynamic>($"{PostBaseUrl}/getPostFull?gids=2&post_id={postId}&read=1", true));
            return resp.Data;
        }
    }
}