using DGP.Genshin.DataModels.Helpers;

namespace DGP.Genshin.DataModels.Materials.GemStones
{
    public class GemStone : Material
    {
        public GemStone()
        {
            Star = StarHelper.FromRank(5);
        }
    }
}
