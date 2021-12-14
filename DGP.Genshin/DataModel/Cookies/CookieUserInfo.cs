using DGP.Genshin.MiHoYoAPI.UserInfo;

namespace DGP.Genshin.DataModel.Cookies
{
    /// <summary>
    /// Cookie与对应的用户信息
    /// </summary>
    public class CookieUserInfo
    {
        public CookieUserInfo(string cookie, UserInfo userInfo)
        {
            Cookie = cookie;
            UserInfo = userInfo;
        }
        public string Cookie { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}