using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Record
{
    //A standard Mihoyo API response
    internal class Response<T>
    {
        [JsonProperty("retcode")] public int ReturnCode { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("data")] public T Data { get; set; }
    }
}
