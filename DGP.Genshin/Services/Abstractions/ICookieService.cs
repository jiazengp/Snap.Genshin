using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    /// <summary>
    /// 全局Cookie管理服务
    /// </summary>
    [Send(typeof(CookieChangedMessage))]
    public interface ICookieService
    {
        /// <summary>
        /// 定义Cookie池
        /// </summary>
        [Send(typeof(CookieAddedMessage))]
        [Send(typeof(CookieRemovedMessage))]
        public interface ICookiePool : IList<string>
        {
            /// <summary>
            /// 添加Cookie,
            /// 隐藏了基类成员<see cref="ICollection{T}.Add(T)"/>以便发送事件
            /// </summary>
            /// <param name="cookie"></param>
            new void Add(string cookie);

            /// <summary>
            /// 添加或忽略相同的 Cookie
            /// </summary>
            /// <param name="cookie"></param>
            /// <returns>是否成功添加</returns>
            bool AddOrIgnore(string cookie);

            /// <summary>
            /// 移除Cookie,
            /// 隐藏了基类成员<see cref="ICollection{T}.Remove(T)"/>以便发送事件
            /// </summary>
            /// <param name="cookie"></param>
            /// <returns></returns>
            new bool Remove(string cookie);
        }

        /// <summary>
        /// 备选Cookie池
        /// </summary>
        ICookiePool Cookies { get; set; }

        /// <summary>
        /// 当前使用的Cookie, 由 <see cref="ICookieService"/> 保证不为 <see cref="null"/>
        /// </summary>
        string CurrentCookie { get; }

        /// <summary>
        /// 用于在初始化时判断Cookie是否可用
        /// </summary>
        bool IsCookieAvailable { get; }

        /// <summary>
        /// 向Cookie池异步添加Cookie,
        /// 忽略已存在的Cookie,
        /// 不更新 <see cref="CurrentCookie"/> 的值
        /// </summary>
        Task AddCookieToPoolOrIgnoreAsync();

        /// <summary>
        /// 将 <see cref="CurrentCookie"/> 的值设置为 <see cref="Cookies"/> 中查找的值
        /// </summary>
        /// <param name="cookie"></param>
        void ChangeOrIgnoreCurrentCookie(string? cookie);

        /// <summary>
        /// 保存 <see cref="Cookies"/> 内的所有 Cookie 信息
        /// </summary>
        void SaveCookies();

        /// <summary>
        /// 异步设置新的Cookie,
        /// 更新 <see cref="CurrentCookie"/> 的值
        /// </summary>
        Task SetCookieAsync();

        /// <summary>
        /// 保存当前的Cookie
        /// </summary>
        /// <param name="cookie"></param>
        void SaveCookie(string cookie);
    }
}