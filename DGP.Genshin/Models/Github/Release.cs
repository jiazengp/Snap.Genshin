using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.Github
{
    /// <summary>
    /// 表示一个发行版
    /// </summary>
    public class Release
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("tag_name")] public string TagName { get; set; }
        [JsonProperty("target_commitish")] public string TargetCommitish { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("author")] public Person Author { get; set; }
        [JsonProperty("prerelease")] public string Prerelease { get; set; }
        [JsonProperty("created_at")] public string CreatedAt { get; set; }
        [JsonProperty("assets")] public List<Asset> Assets { get; set; }
        [JsonProperty("body")] public string Body { get; set; }
    }
}
