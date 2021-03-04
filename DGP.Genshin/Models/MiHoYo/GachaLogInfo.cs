using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo
{
    public class GachaLogInfo
    {
        [JsonProperty("retcode")] public int ReturnCode { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("data")] public GachaLogData Data { get; set; }
    }
}
