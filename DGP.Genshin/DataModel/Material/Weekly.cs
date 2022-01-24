using DGP.Genshin.DataModel.Helper;

namespace DGP.Genshin.DataModel.Material
{
    public class Weekly : Material
    {
        public Weekly()
        {
            Star = StarHelper.FromRank(5);
        }
    }
}
