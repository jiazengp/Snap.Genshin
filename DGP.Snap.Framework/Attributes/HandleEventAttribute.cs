using System;

namespace DGP.Snap.Framework.Attributes
{
    /// <summary>
    /// 指示该方法为事件委托
    /// 且不应被其他方法再次使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class HandleEventAttribute : Attribute
    {
        public HandleEventAttribute()
        {

        }
    }
}
