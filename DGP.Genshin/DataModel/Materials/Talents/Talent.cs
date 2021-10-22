using DGP.Genshin.DataModel.Helpers;
using System;

namespace DGP.Genshin.DataModel.Materials.Talents
{
    public class Talent : Material
    {
        private const string Freedom = @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_423.png";
        private const string Prosperity = @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_443.png";
        private const string Transience = @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_408.png";

        private const string Resistance = @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_453.png";
        private const string Diligence = @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_413.png";
        private const string Elegance = @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_418.png";

        private const string Ballad = "https://genshin.honeyhunterworld.com/img/upgrade/guide/i_403.png";
        private const string Gold = "https://genshin.honeyhunterworld.com/img/upgrade/guide/i_433.png";
        private const string Light = "https://genshin.honeyhunterworld.com/img/upgrade/guide/i_428.png";

        public Talent()
        {
            this.Star = StarHelper.FromRank(4);
        }

        public bool IsTodaysTalent()
        {
            return DateTime.Now.DayOfWeek switch
            {
                DayOfWeek.Sunday => true,
                DayOfWeek.Monday or DayOfWeek.Thursday => this.Source is (Freedom or Prosperity or Transience),
                DayOfWeek.Tuesday or DayOfWeek.Friday => this.Source is (Resistance or Diligence or Elegance),
                DayOfWeek.Wednesday or DayOfWeek.Saturday => this.Source is (Ballad or Gold or Light),
                _ => false,
            };
        }
        public bool IsMondstadt() => this.City == "Mondstadt";
        public bool IsLiyue() => this.City == "Liyue";
        public bool IsInazuma() => this.City == "Inazuma";
    }
}
