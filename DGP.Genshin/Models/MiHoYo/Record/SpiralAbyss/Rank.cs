using DGP.Genshin.Data.Helpers;
using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Record.SpiralAbyss
{
    /// <summary>
    /// 角色数值排行信息
    /// </summary>
    public class Rank
    {
        [JsonProperty("avatar_id")] public int AvatarId { get; set; }
        [JsonProperty("avatar_icon")] public string AvatarIcon { get; set; }
        [JsonProperty("value")] public int Value { get; set; }
        [JsonProperty("rarity")] public int Rarity { get; set; }
        public string StarUrl => StarHelper.FromRank(this.Rarity);
    }
}
