using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Record.Avatar
{
    /// <summary>
    /// 命座信息
    /// </summary>
    internal class Constellation
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
        [JsonProperty("effect")] public string Effect { get; set; }
        public string ProcessedEffect => this.Effect.RemoveHtmlFormat();
        [JsonProperty("is_actived")] public bool IsActived { get; set; }
        [JsonProperty("pos")] public int Position { get; set; }
    }
}
