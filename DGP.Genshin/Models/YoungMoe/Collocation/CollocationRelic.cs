using Newtonsoft.Json;

namespace DGP.Genshin.Models.YoungMoe.Collocation
{
    public class CollocationRelic
    {
        [JsonProperty("id")] public int Id { get; set; }
        /// <summary>
        /// 套装数
        /// </summary>
        [JsonProperty("n")] public int Count { get; set; }
        [JsonProperty("name")] public string? Name { get; set; }
        /// <summary>
        /// 当其中只含有Rate时为使用率
        /// </summary>
        [JsonProperty("rate")] public double Rate { get; set; }
    }

}
