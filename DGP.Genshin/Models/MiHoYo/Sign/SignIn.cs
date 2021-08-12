using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string sign = IsSign ? "已签到" : "未签到";
            return $"签到天数:{TotalSignDay},今日为:{Today},签到情况:{sign}";
        }
    }
    public class SignInResult
    {
        [JsonProperty("code")] public string Code { get; set; }
    }
}
