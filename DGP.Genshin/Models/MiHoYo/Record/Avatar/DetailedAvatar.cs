using DGP.Genshin.DataModel.Helpers;
using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Record.Avatar
{
    /// <summary>
    /// 角色详细详细
    /// </summary>
    [JsonModel]
    public class DetailedAvatar : Avatar
    {
        /// <summary>
        /// we don't want to use this ugly pic here
        /// </summary>
        [Obsolete] [JsonProperty("image")] public new string? Image { get; set; }
        [JsonProperty("icon")] public string? Icon { get; set; }
        [JsonProperty("weapon")] public Weapon? Weapon { get; set; }
        [JsonProperty("reliquaries")] public List<Reliquary>? Reliquaries { get; set; }
        [JsonProperty("constellations")] public List<Constellation>? Constellations { get; set; }
        [JsonProperty("costumes")] public List<Costume>? Costumes { get; set; }
        public string StarUrl => StarHelper.FromRank(this.Rarity);
    }
}
