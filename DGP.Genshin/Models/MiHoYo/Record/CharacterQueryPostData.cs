using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Record
{
    /// <summary>
    /// 请求详细角色数据提交数据
    /// </summary>
    internal class CharacterQueryPostData
    {
        [JsonProperty("character_ids")] public List<int> CharacterIds { get; set; }
        [JsonProperty("role_id")] public string RoleId { get; set; }
        [JsonProperty("server")] public string Server { get; set; }
    }
}
