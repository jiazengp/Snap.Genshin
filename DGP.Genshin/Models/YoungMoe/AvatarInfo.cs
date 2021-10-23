using DGP.Genshin.DataModel.Characters;
using DGP.Genshin.DataModel.Helpers;
using DGP.Genshin.Services;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Windows.Media;

namespace DGP.Genshin.Models.YoungMoe
{
    public class AvatarInfo
    {
        [JsonProperty("avatar")] public string? Avatar { get; set; }
        [JsonProperty("enname")] public string? EnName { get; set; }
        [JsonProperty("star")] public string? Star { get; set; }
        [JsonProperty("element")] public string? ElementType { get; set; }
        /// <summary>
        /// 持有率
        /// </summary>
        [JsonProperty("haveRate")] public double HaveRate { get; set; }
        /// <summary>
        /// 持有且使用率
        /// </summary>
        [Obsolete] [JsonProperty("upRate")] public double UpRate { get; set; }
        /// <summary>
        /// 持有者使用率
        /// </summary>
        [JsonProperty("useRate")] public double UseRate { get; set; }

        #region Injection
        public string? Source => Character?.Source;
        public string? StarUrl => Character?.Star;
        public SolidColorBrush? StarSolid => StarHelper.ToSolid(StarUrl);
        public string? Element => Character?.Element;

        private Character? character;
        private Character Character
        {
            get
            {
                if (character == null)
                {
                    character = Avatar == "旅行者"
                        ? new Character()
                        {
                            Star = StarHelper.FromRank(5),
                            Source = @"https://genshin.honeyhunterworld.com/img/char/traveler_boy_anemo_face_70.png"
                        }
                        : MetaDataService.Instance.Characters.First(c => c.Name == Avatar);
                }
                return character;
            }
        }
        #endregion
    }
}
