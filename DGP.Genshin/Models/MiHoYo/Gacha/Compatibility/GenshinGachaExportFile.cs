using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha.Compatibility
{
    [InterModel]
    public class GenshinGachaExportFile
    {
        [JsonProperty("gachaType")] public List<ConfigType> Types { get; set; }
        [JsonProperty("gachaLog")] public GachaData Data { get; set; }
        [JsonProperty("uid")] public string Uid { get; set; }
    }
}
