using Microsoft.AppCenter.Analytics;
using System;
using System.Collections.Generic;

namespace DGP.Genshin.Helper
{
    /// <summary>
    /// AppCenter 分析事件
    /// 传入的字符串长度不能超过64
    /// </summary>
    internal class Event : Dictionary<string, string>
    {
        /// <summary>
        /// 导航到指定页面
        /// </summary>
        public const string OpenUI = "OpenUI";

        /// <summary>
        /// 打开标题栏
        /// </summary>
        public const string OpenTitle = "OpenTitle";

        /// <summary>
        /// 祈愿记录分析
        /// </summary>
        public const string GachaStatistic = "GachaStatistic";

        /// <summary>
        /// 普通事件的构造器
        /// </summary>
        /// <param name="key">不能为NullOrEmpty</param>
        /// <param name="value"></param>
        public Event(string key, string value)
        {
            this[key] = value;
        }

        /// <summary>
        /// 导航服务使用的事件构造器
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="result">结果</param>
        public Event(Type? type, bool result)
        {
            this[type?.ToString() ?? "Unknown Type"] = result.ToString();
        }

        /// <summary>
        /// 发送自定义事件报告
        /// </summary>
        /// <param name="name">事件名称</param>
        public void TrackAs(string name)
        {
            if (Count > 0)
            {
                Analytics.TrackEvent(name, this);
            }
            else
            {
                Analytics.TrackEvent(name);
            }
        }
    }
}
