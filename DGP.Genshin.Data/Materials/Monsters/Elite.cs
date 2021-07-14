using DGP.Genshin.Data.Helpers;

namespace DGP.Genshin.Data.Materials.Monsters
{
    public class Elite : Material
    {
        public Elite()
        {
            Star = StarHelper.FromRank(4);
        }
    }
}
