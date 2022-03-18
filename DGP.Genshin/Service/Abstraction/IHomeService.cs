using DGP.Genshin.MiHoYoAPI.Announcement;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.Service.Abstraction
{
    public interface IHomeService
    {
        Task<AnnouncementWrapper> GetAnnouncementsAsync(ICommand openAnnouncementUICommand, CancellationToken cancellationToken = default);
        Task<string> GetManifestoAsync(CancellationToken cancellationToken = default);
    }
}
