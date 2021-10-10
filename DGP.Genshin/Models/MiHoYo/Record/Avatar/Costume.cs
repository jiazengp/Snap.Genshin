using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Record.Avatar
{
    /// <summary>
    /// 角色装扮
    /// </summary>
    [JsonModel]
    public class Costume
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
    }
}
