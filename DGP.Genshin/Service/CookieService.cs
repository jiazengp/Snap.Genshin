using DGP.Genshin.Control.Cookie;
using DGP.Genshin.DataModel.Cookie;
using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.UserInfo;
using DGP.Genshin.Service.Abstraction;
using Microsoft;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.Threading;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Data.Json;
using Snap.Data.Primitive;
using Snap.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Service
{

    /// <summary>
    /// TODO 添加 current cookie 启动时校验
    /// </summary>
    [Service(typeof(ICookieService), InjectAs.Singleton)]
    internal class CookieService : ICookieService
    {
        private const string CookieFile = "cookie.dat";
        private const string CookieListFile = "cookielist.dat";

        private static string? currentCookie;

        private readonly IMessenger messenger;

        /// <summary>
        /// Cookie池的默认实现，提供Cookie操作事件支持
        /// Cookie池是非线程安全的对象，需要外围操作确保线程安全
        /// </summary>
        internal class CookiePool : List<string>, ICookieService.ICookiePool
        {
            private readonly List<string> AccountIds = new();
            private readonly ICookieService cookieService;
            private readonly IMessenger messenger;

            /// <summary>
            /// 构造新的 Cookie 池的默认实例
            /// </summary>
            /// <param name="cookieService"></param>
            /// <param name="collection"></param>
            public CookiePool(ICookieService cookieService, IMessenger messenger, IEnumerable<string> collection) : base(collection)
            {
                this.cookieService = cookieService;
                this.messenger = messenger;
                this.AccountIds.AddRange(collection.Select(item => this.GetCookiePairs(item)["account_id"]));
            }

            public new void Add(string cookie)
            {
                if (!string.IsNullOrEmpty(cookie))
                {
                    base.Add(cookie);
                    this.messenger.Send(new CookieAddedMessage(cookie));
                    this.cookieService.SaveCookies();
                }
            }

            public bool AddOrIgnore(string cookie)
            {
                if (this.GetCookiePairs(cookie).TryGetValue("account_id", out string? id))
                {
                    if (!this.AccountIds.Contains(id))
                    {
                        this.AccountIds.Add(id);
                        this.Add(cookie);
                        return true;
                    }
                }
                return false;
            }

            public new bool Remove(string cookie)
            {
                string id = this.GetCookiePairs(cookie)["account_id"];
                this.AccountIds.Remove(id);
                bool result = base.Remove(cookie);
                this.messenger.Send(new CookieRemovedMessage(cookie));
                this.cookieService.SaveCookies();
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

        public AsyncReaderWriterLock CookiesLock { get; init; }

        public CookieService(JoinableTaskContext joinableTaskContext, IMessenger messenger)
        {
            this.messenger = messenger;

            this.CookiesLock = new(joinableTaskContext);

            this.LoadCookies();
            this.LoadCookie();
        }

        /// <summary>
        /// 加载 cookie.dat 文件
        /// </summary>
        private void LoadCookie()
        {
            //load cookie
            string? cookieFile = PathContext.Locate(CookieFile);
            if (File.Exists(cookieFile))
            {
                this.CurrentCookie = File.ReadAllText(cookieFile);
            }
            else
            {
                this.Log("无可用的Cookie");
                File.Create(cookieFile).Dispose();
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
                this.Cookies = new CookiePool(this, this.messenger, base64Cookies.Select(b => Base64Converter.Base64Decode(Encoding.UTF8, b)));
            }
            catch (FileNotFoundException) { }
            catch (Exception ex) { Crashes.TrackError(ex); }
            this.Cookies ??= new CookiePool(this, this.messenger, new List<string>());
        }

        public string CurrentCookie
        {
            get => Requires.NotNull(currentCookie!, nameof(currentCookie));

            private set
            {
                if (currentCookie == value)
                {
                    return;
                }
                currentCookie = value;

                try
                {
                    this.SaveCookie(value);
                    this.Cookies.AddOrIgnore(currentCookie);
                    this.messenger.Send(new CookieChangedMessage(currentCookie));
                }
                catch (UnauthorizedAccessException)
                {
                    Verify.FailOperation("Snap Genshin 无法访问当前目录，请将应用程序移动到别处。");
                }
            }
        }

        public bool IsCookieAvailable
        {
            get => this.initialization.IsCompleted && (!string.IsNullOrEmpty(currentCookie));
        }

        public async Task SetCookieAsync()
        {
            (bool isOk, string cookie) = await new CookieDialog().GetInputCookieAsync();
            if (isOk)
            {
                //prevent user input unexpected invalid cookie
                if (!string.IsNullOrEmpty(cookie) && await this.ValidateCookieAsync(cookie))
                {
                    this.CurrentCookie = cookie;
                }
                File.WriteAllText(PathContext.Locate(CookieFile), this.CurrentCookie);
            }
        }

        public void ChangeOrIgnoreCurrentCookie(string? cookie)
        {
            if (cookie is not null)
            {
                this.CurrentCookie = cookie;
            }
        }

        public async Task AddCookieToPoolOrIgnoreAsync()
        {
            (bool isOk, string newCookie) = await new CookieDialog().GetInputCookieAsync();

            if (isOk)
            {
                if (await this.ValidateCookieAsync(newCookie))
                {
                    using (await this.CookiesLock.WriteLockAsync())
                    {
                        this.Cookies.AddOrIgnore(newCookie);
                    }
                }
            }
        }

        public void SaveCookie(string cookie)
        {
            try
            {
                File.WriteAllText(PathContext.Locate(CookieFile), cookie);
            }
            catch (IOException)
            {
                Verify.FailOperation("cookie.dat 文件被占用，保存cookie失败");
            }
        }

        public void SaveCookies()
        {
            IEnumerable<string> encodedCookies = this.Cookies.Select(c => Base64Converter.Base64Encode(Encoding.UTF8, c));
            Json.ToFile(PathContext.Locate(CookieListFile), encodedCookies);
        }

        /// <summary>
        /// 验证Cookie是否任处于登录态
        /// 若Cookie已失效则会弹出对话框提示用户
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        private async Task<bool> ValidateCookieAsync(string cookie, bool showDialog = true)
        {
            UserInfo? info = await new UserInfoProvider(cookie).GetUserInfoAsync();
            if (info is not null)
            {
                return true;
            }
            else
            {
                if (showDialog)
                {
                    await new ContentDialog
                    {
                        Title = "该Cookie无效",
                        Content = "无法获取到你的账户信息，\n可能是Cookie已经失效，请重新登录获取",
                        PrimaryButtonText = "确认",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
                return false;
            }
        }

        /// <summary>
        /// prevent multiple times initializaion
        /// </summary>
        private readonly WorkWatcher initialization = new(false);
        public async Task InitializeAsync()
        {
            if (this.initialization.IsWorking || this.initialization.IsCompleted)
            {
                return;
            }
            using (this.initialization.Watch())
            {
                using (await this.CookiesLock.WriteLockAsync())
                {
                    //enumerate the shallow copied list to remove item in foreach loop
                    //prevent InvalidOperationException
                    foreach (string cookie in this.Cookies.ToList())
                    {
                        UserInfo? info = await new UserInfoProvider(cookie).GetUserInfoAsync();
                        if (info is null)
                        {
                            //删除用户无法手动选中的cookie(失效的cookie)
                            this.Cookies.Remove(cookie);
                        }
                    }

                    if (this.Cookies.Count <= 0)
                    {
                        currentCookie = null;
                    }
                }
            }
        }

        public async Task<IEnumerable<CookieUserGameRole>> GetCookieUserGameRolesOfAsync(string cookie)
        {
            List<UserGameRole> userGameRoles = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();
            return userGameRoles.Select(u => new CookieUserGameRole(cookie, u));
        }
    }
}