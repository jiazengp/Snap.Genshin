using System;

namespace DGP.Snap.Framework.Attributes
{
    /// <summary>
    /// 指示该方法为事件委托
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class HandleEventAttribute : Attribute
    {
        public HandleEventAttribute()
        {

        }
    }
}
