using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha
{
    /// <summary>
    /// 包装一个储存有祈愿记录的字典
    /// </summary>
    public class GachaData : Dictionary<string, List<GachaLogItem>>
    {
        //public Dictionary<string, List<GachaLogItem>> GachaLogs { get; set; } = new Dictionary<string, List<GachaLogItem>>();
    }
}
