using DGP.Genshin.Services.Cookies;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface ICookieService
    {
        ICookiePool Cookies { get; set; }
        string CurrentCookie { get; }
        bool IsCookieAvailable { get; }

        Task AddNewCookieToPoolAsync();
        void SaveCookies();
        Task SetCookieAsync();
    }
}