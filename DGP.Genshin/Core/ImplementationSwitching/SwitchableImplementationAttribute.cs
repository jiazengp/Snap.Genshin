using Microsoft;
using System;

namespace DGP.Genshin.Core.ImplementationSwitching
{
    /// <summary>
    /// 表示该类作为可切换的实现注入切换上下文
    /// 类型不会被注入到依赖容器中，而是根据默认的公共无参构造器生成实例
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SwitchableImplementationAttribute : Attribute
    {
        internal const string DefaultName = "Snap.Genshin.Default";
        internal const string DefaultDescription = "Snap Genshin 默认实现";
        public SwitchableImplementationAttribute(Type targetInterface, string name, string description)
        {
            TargetType = targetInterface;
            Requires.Argument(name != DefaultName, nameof(name), "注册的名称不能与默认实现名称相同");
            Name = name;
            Requires.Argument(description != DefaultDescription, nameof(name), "注册的描述不能与默认实现描述相同");
            Description = description;
        }

        internal SwitchableImplementationAttribute(Type targetInterface)
        {
            TargetType = targetInterface;
            Name = DefaultName;
            Description = DefaultDescription;
        }

        internal Type TargetType { get; }
        internal string Name { get; }
        internal string Description { get; }
    }
}
