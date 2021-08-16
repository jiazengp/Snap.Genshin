using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo
{
    public class Result
    {
        /// <summary>
        /// 0 is OK
        /// </summary>
        [JsonProperty("retcode")] public int ReturnCode { get; set; }
        [JsonProperty("message")] public string Message { get; set; }

        public override string ToString()
        {
            return $"状态：{ReturnCode} | 信息：{Message}";
        }
    }
}
