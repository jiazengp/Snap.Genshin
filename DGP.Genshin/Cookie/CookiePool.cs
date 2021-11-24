using System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Cookie
{
    /// <summary>
    /// Cookie池，提供Cookie操作事件支持
    /// </summary>
    public class CookiePool : List<string>
    {
        private readonly List<string> AccountIds = new();
        public CookiePool() : base() { }
        public CookiePool(IEnumerable<string> collection) : base(collection)
        {
            AccountIds.AddRange(collection.Select(item => GetCookiePairs(item)["account_id"]));
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="cookie"></param>
        public new void Add(string cookie)
        {
            if (!string.IsNullOrEmpty(cookie))
            {
                base.Add(cookie);
                CookieAdded?.Invoke(cookie);
                CookieManager.SaveCookies();
            }
        }

        /// <summary>
        /// 添加或忽略
        /// </summary>
        /// <param name="cookie"></param>
        public void AddOrIgnore(string cookie)
        {
            if (GetCookiePairs(cookie).TryGetValue("account_id",out string? id))
            {
                if (!AccountIds.Contains(id))
                {
                    AccountIds.Add(id);
                    Add(cookie);
                }
            }
        }

        /// <summary>
        /// 隐藏了基类成员以便发送事件
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public new bool Remove(string cookie)
        {
            string id = GetCookiePairs(cookie)["account_id"];
            AccountIds.Remove(id);
            bool result = base.Remove(cookie);
            CookieRemoved?.Invoke(cookie);
            CookieManager.SaveCookies();
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

        /// <summary>
        /// 获取Cookie的键值对
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        private IDictionary<string, string> GetCookiePairs(string cookie)
        {
            Dictionary<string, string> cookieDictionary = new();

            string[] values = cookie.TrimEnd(';').Split(';');
            foreach (string[] parts in values.Select(c => c.Split(new[] { '=' }, 2)))
            {
                string cookieName = parts[0].Trim();
                string cookieValue;

                if (parts.Length == 1)
                {
                    //Cookie attribute
                    cookieValue = string.Empty;
                }
                else
                {
                    cookieValue = parts[1];
                }

                cookieDictionary[cookieName] = cookieValue;
            }

            return cookieDictionary;
        }
    }
}