using DGP.Genshin.MiHoYoAPI.GameRole;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DGP.Genshin.DataModel.Cookie
{
    /// <summary>
    /// Cookie与对应的某个角色信息
    /// 在进行相等比较时需要使用 <see cref="Equals"/> 方法
    /// </summary>
    [SuppressMessage("","CA1067")]
    public class CookieUserGameRole : IEquatable<CookieUserGameRole>
    {
        public CookieUserGameRole(string cookie, UserGameRole userGameRole)
        {
            Cookie = cookie;
            UserGameRole = userGameRole;
        }
        public string Cookie { get; init; }
        public UserGameRole UserGameRole { get; init; }

        public bool Equals(CookieUserGameRole? other)
        {
            if (other == null)
            {
                return false;
            }
            return Cookie == other.Cookie && UserGameRole == other.UserGameRole;
        }
    }
}