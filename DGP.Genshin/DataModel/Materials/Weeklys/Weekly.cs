using DGP.Genshin.DataModel.Helpers;

namespace DGP.Genshin.DataModel.Materials.Weeklys
{
    public class Weekly : Material
    {
        public Weekly()
        {
            Star = StarHelper.FromRank(5);
        }
    }
}
