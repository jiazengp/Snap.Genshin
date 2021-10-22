using DGP.Genshin.DataModel.Helpers;

namespace DGP.Genshin.DataModel.Materials.GemStones
{
    public class GemStone : Material
    {
        public GemStone()
        {
            this.Star = StarHelper.FromRank(5);
        }
    }
}
