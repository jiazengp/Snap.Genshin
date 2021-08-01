using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo
{
    //A standard Mihoyo API response
    public class Response<T>
    {
        /// <summary>
        /// 0 is OK
        /// </summary>
        [JsonProperty("retcode")] public int ReturnCode { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("data")] public T Data { get; set; }
    }
}
