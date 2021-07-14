using DGP.Genshin.Data.Helpers;

namespace DGP.Genshin.Data.Materials.Monsters
{
    public class Boss : Material
    {
        public Boss()
        {
            Star = StarHelper.FromRank(4);
        }
    }
}
