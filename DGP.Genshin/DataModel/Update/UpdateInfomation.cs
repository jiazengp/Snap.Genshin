using Newtonsoft.Json;
using System;

namespace DGP.Genshin.DataModel.Update
{
    public class UpdateInfomation
    {
        [JsonProperty("body")] public string? ReleaseNote { get; set; }
        [JsonProperty("browser_download_url")] public Uri? PackageUrl { get; set; }
        [JsonProperty("tag_name")] public string Version { get; set; } = null!;
    }
}
