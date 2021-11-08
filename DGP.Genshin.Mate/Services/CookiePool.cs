using System.Collections.Generic;

namespace DGP.Genshin.Mate.Services
{
    /// <summary>
    /// Cookie池，提供Cookie操作事件支持
    /// </summary>
    public class CookiePool : List<string>
    {
        public CookiePool() : base() { }
        public CookiePool(IEnumerable<string> collection) : base(collection) { }
    }
}