using DGP.Genshin.DataModel.Helper;
using System;

namespace DGP.Genshin.DataModel.Material
{
    public class Talent : Material
    {
        public const string Freedom = @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_423.png";
        public const string Prosperity = @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_443.png";
        public const string Transience = @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_408.png";

        public const string Resistance = @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_453.png";
        public const string Diligence = @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_413.png";
        public const string Elegance = @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_418.png";

        public const string Ballad = "https://genshin.honeyhunterworld.com/img/upgrade/guide/i_403.png";
        public const string Gold = "https://genshin.honeyhunterworld.com/img/upgrade/guide/i_433.png";
        public const string Light = "https://genshin.honeyhunterworld.com/img/upgrade/guide/i_428.png";

        public Talent()
        {
            this.Star = StarHelper.FromInt32Rank(4);
        }

        public bool IsTodaysTalent(DayOfWeek? dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => true,
                DayOfWeek.Monday or DayOfWeek.Thursday => this.Source is Freedom or Prosperity or Transience,
                DayOfWeek.Tuesday or DayOfWeek.Friday => this.Source is Resistance or Diligence or Elegance,
                DayOfWeek.Wednesday or DayOfWeek.Saturday => this.Source is Ballad or Gold or Light,
                _ => false,
            };
        }
    }
}
