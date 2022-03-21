using System;

namespace DGP.Genshin.Core.ImplementationSwitching
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class SwitchableInterfaceTypeAttribute : Attribute
    {
        public SwitchableInterfaceTypeAttribute(Type type)
        {
            Type = type;
        }
        public Type Type { get; }
    }
}
