using DGP.Genshin.DataModel.Helper;

namespace DGP.Genshin.DataModel.Material
{
    public class Monster : Material
    {
        public Monster()
        {
            Star = StarHelper.FromRank(3);
        }
    }
}
