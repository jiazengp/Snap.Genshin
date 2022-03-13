using DGP.Genshin.DataModel.Promotion;
using DGP.Genshin.MiHoYoAPI.Calculation;
using System.Collections.Generic;

namespace DGP.Genshin.Service.Abstraction
{
    public interface IMaterialListService
    {
        IEnumerable<ConsumeItem> GetTotalConsumption(MaterialList? materialList);
        MaterialList Load();
        void Save(MaterialList? materialList);
    }
}
