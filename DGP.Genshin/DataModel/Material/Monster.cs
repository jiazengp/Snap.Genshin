using DGP.Genshin.DataModel.Helper;

namespace DGP.Genshin.DataModel.Material
{
    public class Monster : Material
    {
        public Monster()
        {
            Star = StarHelper.FromInt32Rank(3);
        }
    }
}
