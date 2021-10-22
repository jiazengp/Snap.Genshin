using DGP.Genshin.DataModel.Helpers;

namespace DGP.Genshin.DataModel.Materials.Monsters
{
    public class Monster : Material
    {
        public Monster()
        {
            this.Star = StarHelper.FromRank(3);
        }
    }
}
