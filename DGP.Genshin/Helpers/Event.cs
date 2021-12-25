using Microsoft.AppCenter.Analytics;
using System;
using System.Collections.Generic;

namespace DGP.Genshin.Helpers
{
    /// <summary>
    /// AppCenter 分析事件
    /// </summary>
    internal class Event : Dictionary<string, string>
    {
        public const string OpenUI = "OpenUI";
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
        /// <param name="pageType">页面类型</param>
        /// <param name="result">导航的结果</param>
        public Event(Type pageType, bool result)
        {
            this[pageType.ToString()] = result.ToString();
        }

        /// <summary>
        /// 发送自定义事件报告
        /// </summary>
        /// <param name="name">事件名称</param>
        public void TrackAs(string name)
        {
            Analytics.TrackEvent(name, this);
        }
    }
}
