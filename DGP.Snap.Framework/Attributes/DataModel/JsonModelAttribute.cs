using System;

namespace DGP.Snap.Framework.Attributes.DataModel
{
    /// <summary>
    /// 指示此类为Json序列化模型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class JsonModelAttribute : Attribute
    {
        public JsonModelAttribute()
        {

        }
    }
}
