using System;

namespace DGP.Snap.Framework.Attributes.DataModel
{
    /// <summary>
    /// 指示此类为经过处理的中间数据模型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InterModelAttribute : Attribute
    {
        public InterModelAttribute()
        {

        }
    }
}
