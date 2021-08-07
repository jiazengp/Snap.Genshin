using DGP.Genshin.Data.Helpers;
using DGP.Genshin.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Models.MiHoYo.Record.Avatar
{
    internal class DetailedAvatar : Avatar
    {
        /// <summary>
        /// we dont want to use this ugly pic here
        /// </summary>
        [Obsolete] [JsonProperty("image")] public new string Image { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
        public string ConvertedIcon
        {
            get
            {
                var result = DataService.Instance.Characters.FirstOrDefault(c => c.Name == this.Name);
                return result == null ? Icon : result.Source;
            }
        }

        [JsonProperty("weapon")] public Weapon Weapon { get; set; }
        [JsonProperty("reliquaries")] public List<Reliquary> Reliquaries { get; set; }
        [JsonProperty("constellations")] public List<Constellation> Constellations { get; set; }
        [JsonProperty("costumes")] public List<Costume> Costumes { get; set; }
        public string StarUrl => StarHelper.FromRank(this.Rarity);
    }
}
