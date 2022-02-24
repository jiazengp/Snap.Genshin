using DGP.Genshin.DataModel.Cookie;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;

namespace DGP.Genshin.Service.Abstraction
{
    public interface IDailyNoteService
    {
        DailyNote? GetDailyNote(CookieUserGameRole cookieUserGameRole);
        void Initialize();
        void UnInitialize();
    }
}
