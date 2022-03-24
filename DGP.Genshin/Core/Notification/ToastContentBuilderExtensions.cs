using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Runtime.InteropServices;

namespace DGP.Genshin.Core.Notification
{
    /// <summary>
    /// 提供通知的安全调用方法
    /// </summary>
    internal static class ToastContentBuilderExtensions
    {
        /// <summary>
        /// 安全的在主线程上显示通知
        /// 该方法会额外的检查系统版本
        /// </summary>
        /// <param name="builder"></param>
        public static void SafeShow(this ToastContentBuilder builder, bool delegateToMainThread = true)
        {
            //skip windows 7
            if (Environment.OSVersion.Version.Major > 6)
            {
                try
                {
                    if (delegateToMainThread)
                    {
                        App.Current.Dispatcher.Invoke(builder.Show);
                    }
                    else
                    {
                        builder.Show();
                    }
                }
                catch (COMException) { }
                catch (InvalidCastException) { }
                catch (UnauthorizedAccessException) { }
                catch (ArgumentException) { }//this exception seems uncatchable
            }
        }

        /// <summary>
        /// 安全的在主线程上显示通知
        /// 该方法会额外的检查系统版本
        /// </summary>
        /// <param name="builder"></param>
        public static void SafeShow(this ToastContentBuilder builder, CustomizeToast customizeToast, bool delegateToMainThread = true)
        {
            //skip windows 7
            if (Environment.OSVersion.Version.Major > 6)
            {
                try
                {
                    if (delegateToMainThread)
                    {
                        App.Current.Dispatcher.Invoke(() => builder.Show(customizeToast));
                    }
                    else
                    {
                        builder.Show();
                    }
                }
                catch (COMException) { }
                catch (InvalidCastException) { }
                catch (UnauthorizedAccessException) { }
                catch (ArgumentException) { }//this exception seems uncatchable
            }
        }
    }
}