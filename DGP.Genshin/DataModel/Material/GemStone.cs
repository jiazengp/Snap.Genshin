using DGP.Genshin.DataModel.Helper;

namespace DGP.Genshin.DataModel.Material
{
    public class GemStone : Material
    {
        public GemStone()
        {
            Star = StarHelper.FromRank(5);
        }
    }
}
