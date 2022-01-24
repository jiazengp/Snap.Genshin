using DGP.Genshin.DataModel.Helper;

namespace DGP.Genshin.DataModel.Material
{
    public class Elite : Material
    {
        public Elite()
        {
            Star = StarHelper.FromRank(4);
        }
    }
}
