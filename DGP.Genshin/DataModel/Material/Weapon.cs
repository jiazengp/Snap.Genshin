using DGP.Genshin.DataModel.Helper;

namespace DGP.Genshin.DataModel.Material
{
    public class Weapon : Material
    {
        public const string Decarabian = "https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_504.png";
        public const string DandelionGladiator = "https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_544.png";
        public const string BorealWolf = "https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_524.png";

        public const string Guyun = "https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_514.png";
        public const string MistVeiled = "https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_534.png";
        public const string Aerosiderite = "https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_554.png";

        public const string DistantSea = "https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_564.png";
        public const string Narukami = "https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_574.png";
        public const string Mask = "https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_584.png";

        public Weapon()
        {
            this.Star = StarHelper.FromInt32Rank(5);
        }
    }
}
