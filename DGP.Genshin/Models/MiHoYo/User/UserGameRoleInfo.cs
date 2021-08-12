using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.User
{
    public class UserGameRoleInfo
    {
        [JsonProperty("list")] public List<UserGameRole> List { get; set; } = new List<UserGameRole>();
    }
}
