using DGP.Genshin.Data.Helpers;

namespace DGP.Genshin.Data.Materials.Locals
{
    public class Local : Material
    {
        public Local()
        {
            Star = StarHelper.FromRank(1);
        }
    }
}
