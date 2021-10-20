using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Record.Avatar
{
    /// <summary>
    /// 包装详细角色信息列表
    /// </summary>
    [JsonModel]
    public class DetailedAvatarInfo
    {
        [JsonProperty("avatars")] public List<DetailedAvatar>? Avatars { get; set; }
    }
}
