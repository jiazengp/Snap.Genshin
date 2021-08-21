using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.BBSAPI.Post
{
    [JsonModel]
    public class PostWrapper
    {
        [JsonProperty("list")] public List<Post> List { get; set; }
    }
}
