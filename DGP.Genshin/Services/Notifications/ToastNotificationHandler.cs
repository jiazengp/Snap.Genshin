using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Services.Updating;
using Microsoft.Toolkit.Uwp.Notifications;

namespace DGP.Genshin.Services.Notifications
{
    public class ToastNotificationHandler
    {
        /// <summary>
        /// 在后台处理并响应通知
        /// </summary>
        /// <param name="toastArgs"></param>
        internal async void OnActivatedByNotification(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            this.Log(Json.Stringify(toastArgs));
            ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
            if (args.TryGetValue("action", out string value) && value == "update")
            {
                await UpdateService.Instance.DownloadAndInstallPackageAsync();
            }
        }
    }
}