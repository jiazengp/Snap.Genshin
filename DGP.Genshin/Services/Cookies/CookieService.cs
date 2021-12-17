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
        /// 备选Cookie池
        /// </summary>
        public ICookiePool Cookies { get; set; }

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