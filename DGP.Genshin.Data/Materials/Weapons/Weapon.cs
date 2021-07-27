using DGP.Genshin.Data.Helpers;
using System;

namespace DGP.Genshin.Data.Materials.Weapons
{
    public class Weapon : Material
    {
        private const string Decarabian = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_504_70.png";
        private const string DandelionGladiator = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_544_70.png";
        private const string BorealWolf = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_524_70.png";

        private const string Guyun = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_514_70.png";
        private const string MistVeiled = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_534_70.png";
        private const string Aerosiderite = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_554_70.png";

        private const string DistantSea = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_564_70.png";
        private const string Narukami = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_574_70.png";
        private const string Mask = @"https://genshin.honeyhunterworld.com/img/upgrade/weapon/i_584_70.png";

        public Weapon()
        {
            this.Star = StarHelper.FromRank(5);
        }

        public bool IsTodaysWeapon()
        {
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return true;
                case DayOfWeek.Monday:
                case DayOfWeek.Thursday:
                    return
                        this.Source == Decarabian ||
                        this.Source == Guyun ||
                        this.Source == DistantSea;
                case DayOfWeek.Tuesday:
                case DayOfWeek.Friday:
                    return
                        this.Source == BorealWolf ||
                        this.Source == MistVeiled ||
                        this.Source == Narukami;
                case DayOfWeek.Wednesday:
                case DayOfWeek.Saturday:
                    return
                        this.Source == DandelionGladiator ||
                        this.Source == Aerosiderite ||
                        this.Source == Mask;
                default:
                    return false;
            }
        }

        public bool IsMondstadt()
        {
            return
                this.Source == BorealWolf ||
                this.Source == Decarabian ||
                this.Source == DandelionGladiator;
        }
        public bool IsLiyue()
        {
            return
                this.Source == Aerosiderite ||
                this.Source == Guyun ||
                this.Source == MistVeiled;
        }
        public bool IsInazuma()
        {
            return
                this.Source == DistantSea ||
                this.Source == Narukami ||
                this.Source == Mask;
        }
    }
}
