using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo
{
    [JsonModel]
    public class ListWrapper<T>
    {
        [JsonProperty("list")] public List<T> List { get; set; }
    }
}
