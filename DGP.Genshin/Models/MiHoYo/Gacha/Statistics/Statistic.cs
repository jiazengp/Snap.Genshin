using DGP.Snap.Framework.Data.Behavior;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha.Statistics
{
    public class Statistic : Observable
    {
        public string Uid { get; set; }
        public StatisticBanner Permanent { get; set; }
        public StatisticBanner CharacterEvent { get; set; }
        public StatisticBanner WeaponEvent { get; set; }

        public List<StatisticItem> Characters { get; set; }
        public List<StatisticItem> Weapons { get; set; }
    }
}
