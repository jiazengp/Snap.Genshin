using DGP.Genshin.DataModels.Helpers;

namespace DGP.Genshin.DataModels.Materials.Monsters
{
    public class Monster : Material
    {
        public Monster()
        {
            Star = StarHelper.FromRank(3);
        }
    }
}
