using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Gacha
{
    public class GachaLogInfo
    {
        [JsonProperty("retcode")] public int ReturnCode { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("data")] public GachaLogData Data { get; set; }
    }
}
