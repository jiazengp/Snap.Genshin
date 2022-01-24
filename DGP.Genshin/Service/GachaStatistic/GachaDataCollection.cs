using DGP.Genshin.DataModel.GachaStatistic;
using DGP.Genshin.MiHoYoAPI.Gacha;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DGP.Genshin.Service.GachaStatistic
{
    /// <summary>
    /// 包装了包含Uid与抽卡记录的字典
    /// 所有与抽卡记录相关的服务都基于对此类的操作
    /// </summary>
    public class GachaDataCollection : ObservableCollection<UidGachaData>
    {
        /// <summary>
        /// 向集合添加数据
        /// 触发uid增加事件，便于前台响应
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="data"></param>
        public void Add(string uid, GachaData data)
        {
            App.Current.Dispatcher.Invoke(() => Add(new(uid, data)));
        }

        public GachaData? this[string uid] => this.FirstOrDefault(x => x.Uid == uid)?.Data;

        public bool HasUid(string uid)
        {
            return this.Any(x => x.Uid == uid);
        }

        /// <summary>
        /// 获取最新的时间戳id
        /// </summary>
        /// <returns>default 0</returns>
        public long GetNewestTimeIdOf(ConfigType type, string? uid)
        {
            string? typeId = type.Key;
            if (uid is null || typeId is null)
            {
                return 0;
            }
            //有uid有卡池记录就读取最新物品的id,否则返回0
            if (this[uid] is GachaData matchedData)
            {
                if (matchedData.ContainsKey(typeId))
                {
                    if (matchedData[typeId] is List<GachaLogItem> item)
                    {
                        if (item.Any())
                        {
                            return item[0].TimeId;
                        }
                    }
                }
            }
            return 0;
        }
    }
}
