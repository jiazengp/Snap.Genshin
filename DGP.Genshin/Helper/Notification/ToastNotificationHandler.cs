using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Uwp.Notifications;
using Snap.Core.Logging;
using Snap.Data.Json;

namespace DGP.Genshin.Helper.Notification
{
    internal class ToastNotificationHandler
    {
        /// <summary>
        /// 在后台处理并响应通知
        /// </summary>
        /// <param name="toastArgs"></param>
        internal async void OnActivatedByNotification(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            this.Log(Json.Stringify(toastArgs));
            ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
            if (args.TryGetValue("action", out string action) && action == "update")
            {
                IUpdateService updateService = App.AutoWired<IUpdateService>();
                if (updateService.Release is not null)
                {
                    await updateService.DownloadAndInstallPackageAsync();
                }
                else
                {
                    SecureToastNotificationContext.TryCatch(() =>
                    new ToastContentBuilder()
                        .AddText("当前无法获取更新信息")
                        .AddText("请重启 Snap Genshin")
                        .Show());
                }
            }
            else if(args.TryGetValue("taskbarhint", out string taskbarhint) && taskbarhint == "hide")
            {
                Setting2.IsTaskBarIconHintDisplay.Set(false);
            }
            else if (args.TryGetValue("launch", out string launch) && launch == "game")
            {
                LaunchOption? launchOption = new()
                {
                    IsBorderless = Setting2.IsBorderless.Get(),
                    IsFullScreen = Setting2.IsFullScreen.Get(),
                    UnlockFPS = App.IsElevated && Setting2.UnlockFPS.Get(),
                    TargetFPS = (int)Setting2.TargetFPS.Get(),
                    ScreenWidth = (int)Setting2.ScreenWidth.Get(),
                    ScreenHeight = (int)Setting2.ScreenHeight.Get()
                };
                await App.AutoWired<ILaunchService>().LaunchAsync(launchOption, ex =>
                {
                    SecureToastNotificationContext.TryCatch(() =>
                    new ToastContentBuilder()
                        .AddText("启动游戏失败")
                        .AddText(ex.Message)
                        .Show());
                });
            }
        }
    }
}