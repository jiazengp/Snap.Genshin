using System.Collections.Generic;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface ICookieService
    {
        public interface ICookiePool : IList<string>
        {
            new void Add(string cookie);
            void AddOrIgnore(string cookie);
            new bool Remove(string cookie);
        }

        /// <summary>
        /// 备选Cookie池
        /// </summary>
        ICookiePool Cookies { get; set; }
        string CurrentCookie { get; }
        bool IsCookieAvailable { get; }

        Task AddNewCookieToPoolAsync();
        void ChangeOrIgnoreCurrentCookie(string? cookie);
        void SaveCookies();
        Task SetCookieAsync();
    }
}