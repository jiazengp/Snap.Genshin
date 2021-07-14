using DGP.Genshin.Data.Helpers;

namespace DGP.Genshin.Data.Materials.GemStones
{
    public class GemStone : Material
    {
        public GemStone()
        {
            Star = StarHelper.FromRank(5);
        }
    }
}
