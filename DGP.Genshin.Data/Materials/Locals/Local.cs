using DGP.Genshin.Data.Helpers;

namespace DGP.Genshin.Data.Materials.Locals
{
    public class Local : Material
    {
        public Local()
        {
            this.Star = StarHelper.FromRank(1);
        }
    }
}
