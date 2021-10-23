using DGP.Genshin.DataModel.Characters;
using DGP.Genshin.DataModel.Helpers;
using DGP.Genshin.Services;
using Newtonsoft.Json;
using System.Linq;

namespace DGP.Genshin.Models.YoungMoe.Collocation
{
    public class CollocationAvatar
    {
        [JsonProperty("name")] public string? Name { get; set; }
        [JsonProperty("icon")] public string? Icon { get; set; }
        [JsonProperty("rate")] public double? Rate { get; set; }

        public string? Source => Character?.Source;
        public string? StarUrl => Character?.Star;

        public string? Element => Character?.Element;

        private Character? character;
        private Character Character
        {
            get
            {
                if (character == null)
                {
                    character = Name == "旅行者"
                        ? new Character()
                        {
                            Star = StarHelper.FromRank(5),
                            Source = @"https://genshin.honeyhunterworld.com/img/char/traveler_boy_anemo_face_70.png"
                        }
                        : MetaDataService.Instance.Characters.First(c => c.Name == Name);
                }
                return character;
            }
        }
    }
}
