using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Core.Logging;
using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Controls.Cookies;
using DGP.Genshin.Helpers;
using DGP.Genshin.Messages;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Cookies
{
    /// <summary>
    /// 全局Cookie管理服务
    /// </summary>
    [Service(typeof(ICookieService), ServiceType.Singleton)]
    [Send(typeof(CookieChangedMessage))]
    public class CookieService : ICookieService
    {
        private const string CookieFile = "cookie.dat";
        private const string CookieListFile = "cookielist.dat";

        private readonly static object _savingCookie = new();
        private readonly static object _savingCookies = new();

        private static string? currentCookie;

        /// <summary>
        /// Cookie池的默认实现，提供Cookie操作事件支持
        /// </summary>
        [Send(typeof(CookieAddedMeaasge))]
        [Send(typeof(CookieRemovedMessage))]
        internal class CookiePool : List<string>, ICookieService.ICookiePool
        {
            private readonly List<string> AccountIds = new();
            private readonly ICookieService cookieService;
            public CookiePool(ICookieService cookieService, IEnumerable<string> collection) : base(collection)
            {
                this.cookieService = cookieService;
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
                    App.Messenger.Send(new CookieAddedMeaasge(cookie));
                    cookieService.SaveCookies();
                }
            }

            /// <summary>
            /// 添加或忽略
            /// </summary>
            /// <param name="cookie"></param>
            public void AddOrIgnore(string cookie)
            {
                if (GetCookiePairs(cookie).TryGetValue("account_id", out string? id))
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
                App.Messenger.Send(new CookieRemovedMessage(cookie));
                cookieService.SaveCookies();
                return result;
            }

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

        public ICookieService.ICookiePool Cookies { get; set; }

        public CookieService()
        {
            LoadCookies();
            LoadCookie();

            if (currentCookie is not null)
            {
                Cookies.AddOrIgnore(currentCookie);
            }
        }

        private void LoadCookie()
        {
            //load cookie
            if (File.Exists(CookieFile))
            {
                CurrentCookie = File.ReadAllText(CookieFile);
            }
            else
            {
                Logger.LogStatic("无可用的Cookie");
                File.Create(CookieFile).Dispose();
            }
        }

        [MemberNotNull(nameof(Cookies))]
        private void LoadCookies()
        {
            //load cookies
            try
            {
                IEnumerable<string> base64Cookies = Json.FromFile<IEnumerable<string>>(CookieListFile) ?? new List<string>();
                Cookies = new CookiePool(this, base64Cookies.Select(b => Base64Converter.Base64Decode(Encoding.UTF8, b)));
            }
            catch (Exception ex)
            {
                ex.Log($"{ex.Message}");
                Cookies = new CookiePool(this, new List<string>());
                File.Create(CookieListFile).Dispose();
            }
        }

        /// <summary>
        /// 当前使用的Cookie，由<see cref="CookieService"/> 保证不为 <see cref="null"/>
        /// </summary>
        public string CurrentCookie
        {
            get => currentCookie ?? throw new SnapGenshinInternalException("Cookie 不应为 null");
            private set
            {
                if (currentCookie == value)
                {
                    return;
                }
                currentCookie = value;
                if (!Cookies.Contains(currentCookie))
                {
                    Cookies.Add(currentCookie);
                }
                Logger.LogStatic("current cookie has changed");
                App.Messenger.Send(new CookieChangedMessage(currentCookie));
            }
        }

        /// <summary>
        /// 用于在初始化时判断Cookie是否可用
        /// </summary>
        public bool IsCookieAvailable => !string.IsNullOrEmpty(currentCookie);

        /// <summary>
        /// 设置新的Cookie
        /// </summary>
        public async Task SetCookieAsync()
        {
            string cookie = await App.Current.Dispatcher.InvokeAsync(new CookieDialog().GetInputCookieAsync).Task.Unwrap();
            //prevent user input unexpected invalid cookie
            if (cookie != string.Empty)
            {
                CurrentCookie = cookie;
            }
            lock (_savingCookie)
            {
                File.WriteAllText(CookieFile, CurrentCookie);
            }
        }

        public void ChangeOrIgnoreCurrentCookie(string? cookie)
        {
            if (cookie is null)
            {
                return;
            }
            if (CurrentCookie != cookie)
            {
                CurrentCookie = cookie;
            }
        }

        public async Task AddNewCookieToPoolAsync()
        {
            string newCookie = await App.Current.Dispatcher.InvokeAsync(new CookieDialog().GetInputCookieAsync).Task.Unwrap();

            if (!string.IsNullOrEmpty(newCookie))
            {
                Cookies.AddOrIgnore(newCookie);
            }
        }

        public void SaveCookies()
        {
            lock (_savingCookies)
            {
                IEnumerable<string> encodedCookies = Cookies.Select(c => Base64Converter.Base64Encode(Encoding.UTF8, c));
                Json.ToFile(CookieListFile, encodedCookies);
            }
        }
    }
}