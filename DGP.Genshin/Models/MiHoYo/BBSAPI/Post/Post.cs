using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Models.MiHoYo.BBSAPI.Post
{
    public class Post
    {
        [JsonProperty("post_id")] public string PostId { get; set; }
        [JsonProperty("subject")] public string Subject { get; set; }
        [JsonProperty("banner")] public string Banner { get; set; }
        [JsonProperty("official_type")] public int OfficialType { get; set; }
    }
}
