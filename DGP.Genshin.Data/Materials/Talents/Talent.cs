using DGP.Genshin.Data.Helpers;

namespace DGP.Genshin.Data.Materials.Talents
{
    public class Talent : Material
    {
        public Talent()
        {
            Star = StarHelper.FromRank(4);
        }
    }
}
