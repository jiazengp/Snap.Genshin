using System;

namespace DGP.Genshin.Core.Plugins
{
    /// <summary>
    /// 指示该程序集为Snap Genshin 插件主程序集，使得Snap Genshin 能够正确加载插件
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class SnapGenshinPluginAttribute : Attribute { }
}