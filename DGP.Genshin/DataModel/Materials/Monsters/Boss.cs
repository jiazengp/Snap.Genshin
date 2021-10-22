using DGP.Genshin.DataModel.Helpers;

namespace DGP.Genshin.DataModel.Materials.Monsters
{
    public class Boss : Material
    {
        public Boss()
        {
            this.Star = StarHelper.FromRank(4);
        }
    }
}
