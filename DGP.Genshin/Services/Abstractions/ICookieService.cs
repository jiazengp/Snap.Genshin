using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface ICookieService
    {
        ICookiePool Cookies { get; set; }
        string CurrentCookie { get; }
        bool IsCookieAvailable { get; }

        Task AddNewCookieToPoolAsync();
        void ChangeOrIgnoreCurrentCookie(string? cookie);
        void SaveCookies();
        Task SetCookieAsync();
    }
}