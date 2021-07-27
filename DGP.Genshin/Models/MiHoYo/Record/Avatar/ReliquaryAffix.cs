using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Record.Avatar
{
    internal class ReliquaryAffix
    {
        [JsonProperty("activation_number")] public int ActivationNumber { get; set; }
        [JsonProperty("effect")] public string Effect { get; set; }
    }
}
