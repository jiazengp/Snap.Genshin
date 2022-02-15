using DGP.Genshin.DataModel.HutaoAPI;
using DGP.Genshin.HutaoAPI.GetModel;
using DGP.Genshin.HutaoAPI.PostModel;
using DGP.Genshin.MiHoYoAPI.Response;
using Snap.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DGP.Genshin.Service.Abstratcion
{
    public interface IHutaoStatisticService
    {
        Task GetAllRecordsAndUploadAsync(string cookie, Func<PlayerRecord, Task<bool>> confirmFunc, Func<Response, Task> resultAsyncFunc);
        IEnumerable<Item<IEnumerable<NamedValue<double>>>> GetAvatarConstellations();
        IEnumerable<IndexedListWrapper<Item<double>>> GetAvatarParticipations();
        Task<Overview?> GetOverviewAsync();
        IEnumerable<Item<IEnumerable<NamedValue<Rate<IEnumerable<Item<int>>>>>>> GetReliquaryUsages();
        IEnumerable<Item<IEnumerable<Item<double>>>> GetTeamCollocations();
        IEnumerable<IndexedListWrapper<string, Rate<Two<IEnumerable<GenshinItem>>>>> GetTeamCombinations();
        IEnumerable<Item<IEnumerable<Item<double>>>> GetWeaponUsages();
        Task InitializeAsync();
    }
}
