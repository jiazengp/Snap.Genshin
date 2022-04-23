using Newtonsoft.Json;

namespace DGP.Genshin.DataModel.Achievement.UIAF
{
    /// <summary>
    /// UIAF的列表项
    /// </summary>
    public class UIAFItem
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        [JsonProperty("timestamp")]
        public int TimeStamp { get; set; }

        /// <summary>
        /// 当前进度
        /// </summary>
        [JsonProperty("current")]
        public int? Current { get; set; }
    }
}