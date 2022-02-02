using DGP.Genshin.DataModel.Helper;

namespace DGP.Genshin.DataModel.Material
{
    public class GemStone : Material
    {
        public GemStone()
        {
            Star = StarHelper.FromInt32Rank(5);
        }
    }
}
