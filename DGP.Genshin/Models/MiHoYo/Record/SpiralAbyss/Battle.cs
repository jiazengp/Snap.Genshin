using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Record.SpiralAbyss
{
    /// <summary>
    /// 表示一次战斗
    /// </summary>
    [JsonModel]
    public class Battle
    {
        [JsonProperty("index")] public int Index { get; set; }
        [JsonProperty("timestamp")] public string? Timestamp { get; set; }
        [JsonProperty("avatars")] public List<IconAvatar>? Avatars { get; set; }
        public DateTime? Time
        {
            get
            {
                if (this.Timestamp is not null)
                {
                    DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(this.Timestamp));
                    return dto.ToLocalTime().DateTime;
                }
                return null;
            }
        }
    }
}
