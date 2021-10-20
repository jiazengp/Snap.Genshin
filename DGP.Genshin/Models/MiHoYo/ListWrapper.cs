using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo
{
    /// <summary>
    /// 列表对象包装器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [JsonModel]
    public class ListWrapper<T>
    {
        [JsonProperty("list")] public List<T>? List { get; set; }
    }
}
