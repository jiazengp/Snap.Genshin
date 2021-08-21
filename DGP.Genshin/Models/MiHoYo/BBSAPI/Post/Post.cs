using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.BBSAPI.Post
{
    [JsonModel]
    public class Post
    {
        [JsonProperty("post_id")] public string PostId { get; set; }
        [JsonProperty("subject")] public string Subject { get; set; }
        [JsonProperty("banner")] public string Banner { get; set; }
        [JsonProperty("official_type")] public int OfficialType { get; set; }
    }
}
