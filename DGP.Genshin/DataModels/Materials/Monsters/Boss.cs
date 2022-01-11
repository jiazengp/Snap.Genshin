using DGP.Genshin.DataModels.Helpers;

namespace DGP.Genshin.DataModels.Materials.Monsters
{
    public class Boss : Material
    {
        public Boss()
        {
            Star = StarHelper.FromRank(4);
        }
    }
}
