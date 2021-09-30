using System.Collections.Generic;

namespace DGP.Genshin.Models.YoungMoe.Collocation
{
    public class ProcessedRelic
    {
        public double Rate { get; set; }
        public List<CollocationRelic> Relics { get; set; } = new List<CollocationRelic>();
    }
}
