using DGP.Genshin.Data.Helpers;

namespace DGP.Genshin.Data.Materials.Monsters
{
    public class Monster : Material
    {
        public Monster()
        {
            Star = StarHelper.FromRank(3);
        }
    }
}
