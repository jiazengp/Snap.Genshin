using DGP.Genshin.YoungMoeAPI.Collocation;
using System.Collections.Generic;

namespace DGP.Genshin.DataModel.YoungMoe2
{
    public class ProcessedRelic
    {
        public double Rate { get; set; }
        public List<CollocationRelic> Relics { get; set; } = new();
    }
}
