using CommunityToolkit.Mvvm.Messaging;
using DGP.Genshin.Core.Notification;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Sign;
using DGP.Genshin.Service.Abstraction;
using DGP.Genshin.Service.Abstraction.Setting;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualStudio.Threading;
using Snap.Core.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DGP.Genshin.Service
{
    /// <inheritdoc cref="ISignInService"/>
    [Service(typeof(ISignInService), InjectAs.Singleton)]
    internal class SignInService : IRecipient<DayChangedMessage>, ISignInService
    {
        private readonly ICookieService cookieService;
        private readonly IMessenger messenger;

        /// <summary>
        /// 构造一个新的签到服务
        /// </summary>
        /// <param name="cookieService">Cookie服务</param>
        /// <param name="messenger">消息器</param>
        public SignInService(ICookieService cookieService, IMessenger messenger)
        {
            this.messenger = messenger;
            this.cookieService = cookieService;

            messenger.RegisterAll(this);
        }

        /// <summary>
        /// 释放消息器资源
        /// </summary>
        ~SignInService()
        {
            messenger.UnregisterAll(this);
        }

        /// <inheritdoc/>
        public async Task TrySignAllAccountsRolesInAsync()
        {
            using (await cookieService.CookiesLock.ReadLockAsync())
            {
                foreach (string cookie in cookieService.Cookies)
                {
                    List<UserGameRole> roles = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();
                    foreach (UserGameRole role in roles)
                    {
                        string result = await new SignInProvider(cookie).SignInAsync(role);

                        Setting2.LastAutoSignInTime.Set(DateTime.UtcNow);
                        new ToastContentBuilder()
                            .AddHeader("SIGNIN", "米游社每日签到", "SIGNIN")
                            .AddText(result)
                            .AddAttributionText(role.ToString())
                            .SafeShow(toast => { toast.SuppressPopup = Setting2.SignInSilently; }, false);
                    }

                    // Starting from 2022.4.1 or so
                    // We need always wait 15 seconds to sign another account in.
                    await Task.Delay(15000);
                }
            }
        }

        /// <inheritdoc/>
        public void Receive(DayChangedMessage message)
        {
            if (Setting2.AutoDailySignInOnLaunch)
            {
                TrySignAllAccountsRolesInAsync().Forget();
            }
        }
    }
}