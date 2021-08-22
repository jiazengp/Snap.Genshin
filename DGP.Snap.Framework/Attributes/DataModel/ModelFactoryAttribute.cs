using System;

namespace DGP.Snap.Framework.Attributes.DataModel
{
    /// <summary>
    /// 指示此类为处理数据模型的工厂类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ModelFactoryAttribute : Attribute
    {
        public ModelFactoryAttribute()
        {

        }
    }
}
