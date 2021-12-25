using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Controls.Cookies;
using DGP.Genshin.Helpers;
using DGP.Genshin.Messages;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// <inheritdoc cref="ICookieService"/>
    /// </summary>
    [Service(typeof(ICookieService), ServiceType.Singleton)]
    [Send(typeof(CookieChangedMessage))]
    internal class CookieService : ICookieService
    {
        private const string CookieFile = "cookie.dat";
        private const string CookieListFile = "cookielist.dat";

        private static string? currentCookie;

        /// <summary>
        /// Cookie池的默认实现，提供Cookie操作事件支持
        /// </summary>
        [Send(typeof(CookieAddedMessage))]
        [Send(typeof(CookieRemovedMessage))]
        internal class CookiePool : List<string>, ICookieService.ICookiePool
        {
            private readonly List<string> AccountIds = new();
            private readonly ICookieService cookieService;

            /// <summary>
            /// 构造新的 Cookie 池的默认实例
            /// </summary>
            /// <param name="cookieService"></param>
            /// <param name="collection"></param>
            public CookiePool(ICookieService cookieService, IEnumerable<string> collection) : base(collection)
            {
                this.cookieService = cookieService;
                AccountIds.AddRange(collection.Select(item => GetCookiePairs(item)["account_id"]));
            }

            public new void Add(string cookie)
            {
                if (!string.IsNullOrEmpty(cookie))
                {
                    base.Add(cookie);
                    App.Messenger.Send(new CookieAddedMessage(cookie));
                    cookieService.SaveCookies();
                }
            }

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

        /// <summary>
        /// 加载 cookie.dat 文件
        /// </summary>
        private void LoadCookie()
        {
            //load cookie
            if (File.Exists(CookieFile))
            {
                CurrentCookie = File.ReadAllText(CookieFile);
            }
            else
            {
                this.Log("无可用的Cookie");
                File.Create(CookieFile).Dispose();
            }
        }

        /// <summary>
        /// 加载 cookielist.dat 文件
        /// </summary>
        [MemberNotNull(nameof(Cookies))]
        private void LoadCookies()
        {
            try
            {
                IEnumerable<string> base64Cookies = Json.FromFile<IEnumerable<string>>(CookieListFile) ?? new List<string>();
                Cookies = new CookiePool(this, base64Cookies.Select(b => Base64Converter.Base64Decode(Encoding.UTF8, b)));
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                Cookies = new CookiePool(this, new List<string>());
            }
        }

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
                this.Log("current cookie has changed");
                App.Messenger.Send(new CookieChangedMessage(currentCookie));
            }
        }

        public bool IsCookieAvailable => !string.IsNullOrEmpty(currentCookie);

        public async Task SetCookieAsync()
        {
            string cookie = await App.Current.Dispatcher.InvokeAsync(new CookieDialog().GetInputCookieAsync).Task.Unwrap();
            //prevent user input unexpected invalid cookie
            if (!string.IsNullOrEmpty(cookie))
            {
                CurrentCookie = cookie;
            }
            File.WriteAllText(CookieFile, CurrentCookie);
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

        public async Task AddCookieToPoolOrIgnoreAsync()
        {
            string newCookie = await App.Current.Dispatcher.InvokeAsync(new CookieDialog().GetInputCookieAsync).Task.Unwrap();

            if (!string.IsNullOrEmpty(newCookie))
            {
                Cookies.AddOrIgnore(newCookie);
            }
        }

        public void SaveCookies()
        {
            IEnumerable<string> encodedCookies = Cookies.Select(c => Base64Converter.Base64Encode(Encoding.UTF8, c));
            Json.ToFile(CookieListFile, encodedCookies);
        }
    }
}