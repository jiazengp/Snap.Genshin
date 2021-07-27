using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Record
{
    internal class WorldExploration
    {
        [JsonProperty("level")] public int Level { get; set; }
        /// <summary>
        /// Maxmium is 1000
        /// </summary>
        [JsonProperty("exploration_percentage")] public int ExplorationPercentage { get; set; }
        public double ExplorationPercentageBy10 => this.ExplorationPercentage / 10.0;
        [JsonProperty("icon")] public string Icon { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        public string ConvertedType => this.IsReputation ? "声望等级" : "供奉等级";
        public bool IsReputation => this.Type == "Reputation";
        [JsonProperty("offerings")] public List<Offering> Offerings { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
    }
}
