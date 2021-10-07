using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha.Compatibility
{
    public class ImportableGachaData
    {
        public List<ConfigType> Types { get; set; }
        public GachaData Data { get; set; }
        public string Uid { get; set; }
    }
}
