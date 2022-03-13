using DGP.Genshin.DataModel.Promotion;
using DGP.Genshin.Helper;
using DGP.Genshin.MiHoYoAPI.Calculation;
using DGP.Genshin.Service.Abstraction;
using Snap.Core.DependencyInjection;
using Snap.Data.Json;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Service
{
    [Service(typeof(IMaterialListService), InjectAs.Transient)]
    internal class MaterialListService : IMaterialListService
    {
        private const string MaterialListFileName = "MaterialList.json";

        public MaterialList Load()
        {
            return Json.FromFileOrNew<MaterialList>(PathContext.Locate(MaterialListFileName));
        }

        public void Save(MaterialList? materialList)
        {
            Json.ToFile(MaterialListFileName, materialList);
        }

        public IEnumerable<ConsumeItem> GetTotalConsumption(MaterialList? materialList)
        {
            List<ConsumeItem> totalConsumption = new();
            if (materialList != null)
            {
                foreach (CalculableConsume list in materialList)
                {
                    foreach (ConsumeItem item in list.ConsumeItems)
                    {
                        if (totalConsumption.SingleOrDefault(i => i.Id == item.Id) is ConsumeItem matched)
                        {
                            matched.Num += item.Num;
                        }
                        else
                        {
                            totalConsumption.Add(item.Clone());
                        }
                    }
                }
            }

            return totalConsumption.OrderByDescending(i => i.Num);
        }
    }
}
