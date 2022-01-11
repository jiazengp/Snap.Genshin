using DGP.Genshin.DataModels.Helpers;

namespace DGP.Genshin.DataModels.Materials.Monsters
{
    public class Elite : Material
    {
        public Elite()
        {
            Star = StarHelper.FromRank(4);
        }
    }
}
