using DGP.Genshin.YoungMoeAPI.Collocation;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.DataModel.YoungMoe2
{
    public class DetailedAvatarInfo2 : DetailedAvatarInfo
    {
        public DetailedAvatarInfo2(DetailedAvatarInfo d)
        {
            Avatar = d.Avatar;
            HaveRate = d.HaveRate;
            UpRate = d.UpRate;
            UseRate = d.UseRate;
            AvgLevel = d.AvgLevel;
            Star = d.Star;
            AvgConstellation = d.AvgConstellation;
            CollocationWeapon = d.CollocationWeapon?.Select(w => new CollocationWeapon2(w));
            CollocationAvatar = d.CollocationAvatar?.Select(a => new CollocationAvatar2(a));
            Relics = d.Relics;
            Constellation = d.Constellation;
        }

        public new IEnumerable<CollocationWeapon2>? CollocationWeapon { get; set; }
        public new IEnumerable<CollocationAvatar2>? CollocationAvatar { get; set; }

        private List<ProcessedRelic>? processedRelics;
        public List<ProcessedRelic> ProcessedRelics
        {
            get
            {
                if (processedRelics == null)
                {
                    processedRelics = new();
                    if (Relics is not null)
                    {
                        foreach (List<CollocationRelic> relic in Relics)
                        {
                            ProcessedRelic p = new();
                            foreach (CollocationRelic item in relic)
                            {
                                if (item.Rate != 0)
                                {
                                    p.Rate = item.Rate;
                                }
                                else
                                {
                                    p.Relics.Add(item);
                                }
                            }
                            processedRelics.Add(p);
                        }
                    }
                }
                return processedRelics;
            }
        }
    }
}
