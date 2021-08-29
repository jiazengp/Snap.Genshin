using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Sign
{
    [JsonModel]
    public class SignInInfo
    {
        /// <summary>
        /// 累积签到天数
        /// </summary>
        [JsonProperty("total_sign_day")] public int TotalSignDay { get; set; }
        [JsonProperty("today")] public string Today { get; set; }
        /// <summary>
        /// 今日是否已签到
        /// </summary>
        [JsonProperty("is_sign")] public bool IsSign { get; set; }
        public bool IsNotSign => !IsSign;
        [JsonProperty("is_sub")] public bool IsSub { get; set; }
        [JsonProperty("first_bind")] public bool FirstBind { get; set; }
    }
}
