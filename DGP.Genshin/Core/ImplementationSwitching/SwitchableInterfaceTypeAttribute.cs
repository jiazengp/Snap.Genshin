using System;

namespace DGP.Genshin.Core.ImplementationSwitching
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class SwitchableInterfaceTypeAttribute : Attribute
    {
        public SwitchableInterfaceTypeAttribute(Type type)
        {
            this.Type = type;
        }
        public Type Type { get; }
    }
}
