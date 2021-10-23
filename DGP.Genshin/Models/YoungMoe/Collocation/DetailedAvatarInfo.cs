using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.YoungMoe.Collocation
{
    public class DetailedAvatarInfo : AvatarInfo
    {
        [JsonProperty("avgLevel")] public double AvgLevel { get; set; }
        [JsonProperty("avgConstellation")] public double AvgConstellation { get; set; }
        [JsonProperty("collocationWeapon")] public List<CollocationWeapon>? CollocationWeapon { get; set; }
        [JsonProperty("collocationAvatar")] public List<CollocationAvatar>? CollocationAvatar { get; set; }
        [JsonProperty("relic")] public List<List<CollocationRelic>>? Relics { get; set; }
        [JsonProperty("constellation")] public List<double>? Constellation { get; set; }


        private List<ProcessedRelic>? processedRelics;
        public List<ProcessedRelic> ProcessedRelics
        {
            get
            {
                if (processedRelics == null)
                {
                    processedRelics = new List<ProcessedRelic>();
                    if (Relics is not null)
                    {
                        foreach (List<CollocationRelic> relic in Relics)
                        {
                            ProcessedRelic p = new ProcessedRelic();
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
