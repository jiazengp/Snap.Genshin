using System;

namespace DGP.Genshin.Core.Plugins
{
    /// <summary>
    /// 实现插件接口
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 作者
        /// </summary>
        string Author { get; }

        /// <summary>
        /// 版本
        /// </summary>
        Version Version { get; }
    }
}