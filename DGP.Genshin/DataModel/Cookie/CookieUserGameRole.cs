using DGP.Genshin.MiHoYoAPI.GameRole;
using System;

namespace DGP.Genshin.DataModel.Cookie
{
    public class CookieUserGameRole : IEquatable<CookieUserGameRole>
    {
        public CookieUserGameRole(string cookie, UserGameRole userGameRole)
        {
            Cookie = cookie;
            UserGameRole = userGameRole;
        }
        public string Cookie { get; set; }
        public UserGameRole UserGameRole { get; set; }

        public bool Equals(CookieUserGameRole? other)
        {
            if (other == null)
            {
                return false;
            }
            return Cookie == other.Cookie && UserGameRole == other.UserGameRole;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as CookieUserGameRole);
        }

        /// <summary>
        /// 比较两个 <see cref="CookieUserGameRole"/> 是否相同
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(CookieUserGameRole? left, CookieUserGameRole? right)
        {
            if (left is null || right is null)
            {
                if (left is null && right is null)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return left.Equals(right);
            }
        }

        public static bool operator !=(CookieUserGameRole? left, CookieUserGameRole? right)
        {
            if (left is null || right is null)
            {
                if (left is null && right is null)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return !left.Equals(right);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}