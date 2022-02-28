using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualStudio.Threading;
using Snap.Core.Logging;
using Snap.Data.Json;
using System.Threading.Tasks;

namespace DGP.Genshin.Core.Notification
{
    internal class ToastNotificationHandler
    {
        /// <summary>
        /// 在后台处理并响应通知
        /// </summary>
        /// <param name="toastArgs"></param>
        internal void OnActivatedByNotification(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            HandleNotificationActivationAsync(toastArgs).Forget();
        }

        private async Task HandleNotificationActivationAsync(ToastNotificationActivatedEventArgsCompat toastArgs)
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
                    new ToastContentBuilder()
                        .AddText("当前无法获取更新信息")
                        .AddText("请重启 Snap Genshin")
                        .SafeShow();
                }
            }
            else if (args.TryGetValue("taskbarhint", out string taskbarhint) && taskbarhint == "hide")
            {
                Setting2.IsTaskBarIconHintDisplay.Set(false);
            }
            else if (args.TryGetValue("launch", out string launch))
            {
                ILaunchService launchService = App.AutoWired<ILaunchService>();
                switch (launch)
                {
                    case "game":
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
                            await launchService.LaunchAsync(launchOption, ex =>
                            {
                                new ToastContentBuilder()
                                    .AddText("启动游戏失败")
                                    .AddText(ex.Message)
                                    .SafeShow();
                            });
                            break;
                        }
                    case "launcher":
                        {
                            launchService.OpenOfficialLauncher(ex =>
                            {
                                new ToastContentBuilder()
                                    .AddText("打开启动器失败")
                                    .AddText(ex.Message)
                                    .SafeShow();
                            });
                            break;
                        }
                    default:
                        break;
                }
            }
        }
    }
}