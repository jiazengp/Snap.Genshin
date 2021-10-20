using DGP.Genshin.Data.Helpers;

namespace DGP.Genshin.Data.Materials.Monsters
{
    public class Elite : Material
    {
        public Elite()
        {
            this.Star = StarHelper.FromRank(4);
        }
    }
}
