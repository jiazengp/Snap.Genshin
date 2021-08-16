using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Sign
{
    public class SignIn
    {
        [JsonProperty("total_sign_day")] public int TotalSignDay { get; set; }
        [JsonProperty("today")] public string Today { get; set; }
        [JsonProperty("is_sign")] public bool IsSign { get; set; }
        [JsonProperty("first_bind")] public bool FirstBind { get; set; }

        public override string ToString()
        {
            string sign = this.IsSign ? "已签到" : "未签到";
            return $"签到天数:{this.TotalSignDay},今日为:{this.Today},签到情况:{sign}";
        }
    }
}
