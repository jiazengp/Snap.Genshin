using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha
{
    public class GachaData
    {
        public Dictionary<string, List<GachaLogItem>> GachaLogs { get; set; }
            = new Dictionary<string, List<GachaLogItem>>();
    }
}
