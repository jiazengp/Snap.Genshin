using DGP.Genshin.DataModel.Helpers;
using System;

namespace DGP.Genshin.DataModel.Materials.Weapons
{
    public class Weapon : Material
    {
        private const string Decarabian = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_504.png";
        private const string DandelionGladiator = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_544.png";
        private const string BorealWolf = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_524.png";

        private const string Guyun = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_514.png";
        private const string MistVeiled = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_534.png";
        private const string Aerosiderite = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_554.png";

        private const string DistantSea = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_564.png";
        private const string Narukami = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_574.png";
        private const string Mask = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_584.png";

        public Weapon()
        {
            this.Star = StarHelper.FromRank(5);
        }

        public bool IsTodaysWeapon()
        {
            return DateTime.Now.DayOfWeek switch
            {
                DayOfWeek.Sunday => true,
                DayOfWeek.Monday or DayOfWeek.Thursday => this.Source is (Decarabian or Guyun or DistantSea),
                DayOfWeek.Tuesday or DayOfWeek.Friday => this.Source is (BorealWolf or MistVeiled or Narukami),
                DayOfWeek.Wednesday or DayOfWeek.Saturday => this.Source is (DandelionGladiator or Aerosiderite or Mask),
                _ => false,
            };
        }

        public bool IsMondstadt()
        {
            return
                this.Source is
                BorealWolf or
                Decarabian or
                DandelionGladiator;
        }
        public bool IsLiyue()
        {
            return
                this.Source is
                Aerosiderite or
                Guyun or
                MistVeiled;
        }
        public bool IsInazuma()
        {
            return
                this.Source is
                DistantSea or
                Narukami or
                Mask;
        }
    }
}
