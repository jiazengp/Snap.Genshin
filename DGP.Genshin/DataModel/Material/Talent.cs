using DGP.Genshin.DataModel.Helper;
using System;

namespace DGP.Genshin.DataModel.Material
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
            Star = StarHelper.FromRank(4);
        }

        public bool IsTodaysTalent(DayOfWeek? dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => true,
                DayOfWeek.Monday or DayOfWeek.Thursday => Source is Freedom or Prosperity or Transience,
                DayOfWeek.Tuesday or DayOfWeek.Friday => Source is Resistance or Diligence or Elegance,
                DayOfWeek.Wednesday or DayOfWeek.Saturday => Source is Ballad or Gold or Light,
                _ => false,
            };
        }
        public bool IsMondstadt()
        {
            return City == "Mondstadt";
        }

        public bool IsLiyue()
        {
            return City == "Liyue";
        }

        public bool IsInazuma()
        {
            return City == "Inazuma";
        }
    }
}
