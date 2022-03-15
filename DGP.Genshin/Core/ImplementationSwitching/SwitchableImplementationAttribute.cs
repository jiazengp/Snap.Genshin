using System;

namespace DGP.Genshin.Core.ImplementationSwitching
{
    /// <summary>
    /// 表示该类作为可切换的实现注入切换上下文
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SwitchableImplementationAttribute : Attribute
    {
        public SwitchableImplementationAttribute(Type targetInterface, string description)
        {
            TargetType = targetInterface;
            Description = description;
        }

        internal Type TargetType { get; }
        internal string Description { get; }
    }
}
