using DGP.Genshin.MiHoYoAPI.GameRole;

namespace DGP.Genshin.DataModel.Cookie
{
    /// <summary>
    /// Cookie与对应的某个角色信息
    /// </summary>
    public record CookieUserGameRole
    {
        public CookieUserGameRole(string cookie, UserGameRole userGameRole)
        {
            this.Cookie = cookie;
            this.UserGameRole = userGameRole;
        }
        public string Cookie { get; init; }
        public UserGameRole UserGameRole { get; init; }

        public override string ToString()
        {
            return this.UserGameRole.ToString();
        }
    }
}