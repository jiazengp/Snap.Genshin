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

        public override string ToString() => $"状态：{this.ReturnCode} | 信息：{this.Message}";
    }
}
