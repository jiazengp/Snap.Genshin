using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Record.SpiralAbyss
{
    /// <summary>
    /// 表示一次战斗
    /// </summary>
    public class Battle
    {
        [JsonProperty("index")] public int Index { get; set; }
        [JsonProperty("timestamp")] public string Timestamp { get; set; }
        [JsonProperty("avatars")] public List<IconAvatar> Avatars { get; set; }
        public DateTime Time => TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(Int32.Parse(this.Timestamp));
    }
}
