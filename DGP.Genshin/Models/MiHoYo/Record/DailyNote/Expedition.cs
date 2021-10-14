using Newtonsoft.Json;
using System;

namespace DGP.Genshin.Models.MiHoYo.Record.DailyNote
{
    /// <summary>
    /// 探索派遣
    /// </summary>
    public class Expedition
    {
        /// <summary>
        /// 图标
        /// </summary>
        [JsonProperty("avatar_side_icon")] public string AvatarSideIcon { get; set; }
        /// <summary>
        /// 状态 Ongoing:派遣中
        /// </summary>
        [JsonProperty("status")] public string Status { get; set; }
        /// <summary>
        /// 剩余时间
        /// </summary>
        [JsonProperty("remained_time")] public string RemainedTime { get; set; }

        public string RemainedTimeFormatted
        {
            get
            {
                TimeSpan ts = new TimeSpan(0, 0, int.Parse(RemainedTime));
                return ts.Days > 0 ? $"{ts.Days}天{ts.Hours}时{ts.Minutes}" : $"{ts.Hours}时{ts.Minutes}";
            }
        }
    }
}
