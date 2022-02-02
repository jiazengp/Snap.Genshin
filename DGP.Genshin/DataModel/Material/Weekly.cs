using DGP.Genshin.DataModel.Helper;

namespace DGP.Genshin.DataModel.Material
{
    public class Weekly : Material
    {
        public Weekly()
        {
            Star = StarHelper.FromInt32Rank(5);
        }
    }
}
