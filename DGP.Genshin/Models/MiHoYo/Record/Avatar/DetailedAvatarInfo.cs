using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Record.Avatar
{
    internal class DetailedAvatarInfo
    {
        [JsonProperty("avatars")] public List<DetailedAvatar> Avatars { get; set; }
    }
}
