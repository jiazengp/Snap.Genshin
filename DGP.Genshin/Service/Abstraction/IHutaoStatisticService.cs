using DGP.Genshin.DataModel;
using DGP.Genshin.DataModel.HutaoAPI;
using DGP.Genshin.HutaoAPI.GetModel;
using DGP.Genshin.MiHoYoAPI.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DGP.Genshin.Service.Abstratcion
{
    public interface IHutaoStatisticService
    {
        Task<List<Response>> GetAllRecordsAndUploadAsync(string cookie);
        IEnumerable<IndexedListWrapper<Item<double>>> GetAvatarParticipations();
        Task<Overview?> GetOverviewAsync();
        IEnumerable<Item<IEnumerable<Item<double>>>> GetTeamCollocations();
        IEnumerable<Item<IEnumerable<Item<double>>>> GetWeaponUsages();
        Task InitializeAsync();
    }
}
