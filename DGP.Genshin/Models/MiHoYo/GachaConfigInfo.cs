using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo
{
    public class GachaConfigInfo
    {
        [JsonProperty("retcode")] public int ReturnCode { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("data")] public GachaConfigData Data { get; set; }
    }
}
