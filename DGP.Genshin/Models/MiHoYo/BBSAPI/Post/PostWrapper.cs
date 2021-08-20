using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.BBSAPI.Post
{
    public class PostWrapper
    {
        [JsonProperty("list")] public List<Post> List { get; set; }
    }
}
