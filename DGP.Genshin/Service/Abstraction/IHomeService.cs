using DGP.Genshin.MiHoYoAPI.Announcement;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.Service.Abstraction
{
    public interface IHomeService
    {
        Task<AnnouncementWrapper> GetAnnouncementsAsync(ICommand openAnnouncementUICommand);
        Task<string> GetManifestoAsync();
    }
}
