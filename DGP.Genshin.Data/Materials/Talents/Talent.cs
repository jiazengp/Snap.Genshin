using DGP.Genshin.Data.Helpers;
using System;

namespace DGP.Genshin.Data.Materials.Talents
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
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return true;
                case DayOfWeek.Monday:
                case DayOfWeek.Thursday:
                    return
                        this.Source == Freedom ||//自由
                        this.Source == Prosperity ||//繁荣
                        this.Source == Transience;//浮世
                case DayOfWeek.Tuesday:
                case DayOfWeek.Friday:
                    return
                        this.Source == Resistance ||//抗争
                        this.Source == Diligence ||//勤劳
                        this.Source == Elegance;//风雅
                case DayOfWeek.Wednesday:
                case DayOfWeek.Saturday:
                    return
                        this.Source == Ballad ||//诗文
                        this.Source == Gold ||//黄金
                        this.Source == Light;//天光
                default:
                    return false;
            }
        }
        public bool IsMondstadt() => this.City == "Mondstadt";
        public bool IsLiyue() => this.City == "Liyue";
        public bool IsInazuma() => this.City == "Inazuma";
    }
}
