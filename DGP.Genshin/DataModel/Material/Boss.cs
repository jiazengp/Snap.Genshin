using DGP.Genshin.DataModel.Helper;

namespace DGP.Genshin.DataModel.Material
{
    public class Boss : Material
    {
        public Boss()
        {
            Star = StarHelper.FromInt32Rank(4);
        }
    }
}
