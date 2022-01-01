using DGP.Genshin.MiHoYoAPI.GameRole;

namespace DGP.Genshin.DataModels.Cookies
{
    public class CookieUserGameRole
    {
        public CookieUserGameRole(string cookie, UserGameRole userGameRole)
        {
            Cookie = cookie;
            UserGameRole = userGameRole;
        }
        public string Cookie { get; set; }
        public UserGameRole UserGameRole { get; set; }
    }
}