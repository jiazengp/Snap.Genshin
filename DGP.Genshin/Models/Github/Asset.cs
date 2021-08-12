using Newtonsoft.Json;

namespace DGP.Genshin.Models.Github
{
    /// <summary>
    /// 表示一个资源文件
    /// </summary>
    public class Asset
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("browser_download_url")] public string BrowserDownloadUrl { get; set; }
    }
}
