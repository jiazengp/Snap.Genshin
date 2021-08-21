using DGP.Snap.Framework.Attributes.DataModel;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha
{
    /// <summary>
    /// 包装一个储存有祈愿记录的字典
    /// </summary>
    [InterModel]
    public class GachaData : Dictionary<string, List<GachaLogItem>>
    {
    }
}
