using DGP.Genshin.DataModels.Helpers;

namespace DGP.Genshin.DataModels.Materials.Weeklys
{
    public class Weekly : Material
    {
        public Weekly()
        {
            Star = StarHelper.FromRank(5);
        }
    }
}
