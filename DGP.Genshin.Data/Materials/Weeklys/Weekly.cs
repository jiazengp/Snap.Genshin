using DGP.Genshin.Data.Helpers;

namespace DGP.Genshin.Data.Materials.Weeklys
{
    public class Weekly : Material
    {
        public Weekly()
        {
            Star = StarHelper.FromRank(5);
        }
    }
}
