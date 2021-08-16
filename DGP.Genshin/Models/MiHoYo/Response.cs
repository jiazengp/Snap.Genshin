using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo
{
    /// <summary>
    /// Mihoyo 标准API响应
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class Response<T> : Result
    {
        [JsonProperty("data")] public T Data { get; set; }
    }
}