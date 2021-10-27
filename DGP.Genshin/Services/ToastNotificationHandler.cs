using DGP.Genshin.Services.Updating;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Foundation.Collections;

namespace DGP.Genshin.Services
{
    public class ToastNotificationHandler
    {
        /// <summary>
        /// 在后台处理并响应通知
        /// </summary>
        /// <param name="toastArgs"></param>
        internal async void OnActivatedByNotification(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            //ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
            //if (ToastNotificationManagerCompat.WasCurrentProcessToastActivated())

            ValueSet userInput = toastArgs.UserInput;
            if (userInput.ContainsKey("action"))
            {
                if ((string)userInput["action"] == "update")
                {
                    await App.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        UpdateService.Instance.DownloadAndInstallPackage();
                        await new UpdateDialog().ShowAsync();
                    });
                }
            }
        }
    }
}
