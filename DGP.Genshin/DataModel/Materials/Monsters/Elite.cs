using DGP.Genshin.DataModel.Helpers;

namespace DGP.Genshin.DataModel.Materials.Monsters
{
    public class Elite : Material
    {
        public Elite()
        {
            this.Star = StarHelper.FromRank(4);
        }
    }
}
