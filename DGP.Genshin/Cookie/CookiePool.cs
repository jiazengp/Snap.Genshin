using System;
using System.Collections.Generic;

namespace DGP.Genshin.Cookie
{
    /// <summary>
    /// Cookie池，提供Cookie操作事件支持
    /// </summary>
    public class CookiePool : List<string>
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="cookie"></param>
        public new void Add(string cookie)
        {
            base.Add(cookie);
            CookieAdded?.Invoke(cookie);
        }
        /// <summary>
        /// 添加或忽略
        /// </summary>
        /// <param name="cookie"></param>
        public void AddOrIgnore(string cookie)
        {
            if (!Contains(cookie))
            {
                Add(cookie);
            }
        }

        /// <summary>
        /// 隐藏了基类成员以便发送事件
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public new bool Remove(string cookie)
        {
            bool result = base.Remove(cookie);
            CookieRemoved?.Invoke(cookie);
            return result;
        }

        /// <summary>
        /// 当明确有新的Cookie加入时触发
        /// </summary>
        public event Action<string>? CookieAdded;

        /// <summary>
        /// 当Cookie删除时触发
        /// </summary>
        public event Action<string>? CookieRemoved;
    }
}