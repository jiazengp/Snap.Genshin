using DGP.Genshin.Data.Helpers;
using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Record.Avatar
{
    internal class Reliquary
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
        [JsonProperty("pos")] public int Position { get; set; }
        [JsonProperty("rarity")] public int Rarity { get; set; }
        public string StarUrl => StarHelper.FromRank(this.Rarity);
        [JsonProperty("level")] public int Level { get; set; }
        [JsonProperty("set")] public ReliquarySet ReliquarySet { get; set; }
        [JsonProperty("pos_name")] public string PositionName { get; set; }
    }
}
