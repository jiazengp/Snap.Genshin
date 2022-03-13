using DGP.Genshin.DataModel.HutaoAPI;
using DGP.Genshin.HutaoAPI.GetModel;
using DGP.Genshin.HutaoAPI.PostModel;
using DGP.Genshin.MiHoYoAPI.Response;
using Snap.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DGP.Genshin.Service.Abstraction
{
    public interface IHutaoStatisticService
    {
        Task GetAllRecordsAndUploadAsync(string cookie, Func<PlayerRecord, Task<bool>> confirmFunc, Func<Response, Task> resultAsyncFunc);
        IList<Rate<Item<IList<NamedValue<double>>>>> GetAvatarConstellations();
        IList<Indexed<int, Item<double>>> GetAvatarParticipations();
        Task<Overview?> GetOverviewAsync();
        IList<Item<IList<NamedValue<Rate<IList<Item<int>>>>>>> GetReliquaryUsages();
        IList<Item<IList<Item<double>>>> GetTeamCollocations();
        IList<Indexed<string, Rate<Two<IList<HutaoItem>>>>> GetTeamCombinations();
        IList<Item<IList<Item<double>>>> GetWeaponUsages();
        Task InitializeAsync();
    }
}
