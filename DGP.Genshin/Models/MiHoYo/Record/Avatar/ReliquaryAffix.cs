using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Record.Avatar
{
    /// <summary>
    /// 圣遗物套装效果
    /// </summary>
    internal class ReliquaryAffix
    {
        [JsonProperty("activation_number")] public int ActivationNumber { get; set; }
        [JsonProperty("effect")] public string Effect { get; set; }
    }
}
