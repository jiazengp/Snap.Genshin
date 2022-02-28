using System;
using System.Runtime.InteropServices;

namespace DGP.Genshin.Core.Notification
{
    /// <summary>
    /// 为 Windows Toast 通知 提供安全的执行上下文
    /// </summary>
    internal class SecureToastNotificationContext
    {
        /// <summary>
        /// 在此处执行发送操作是相对安全的
        /// </summary>
        /// <param name="toastOperation"></param>
        [Obsolete("不再推荐使用此方法")]
        internal static void Invoke(Action toastOperation)
        {
            //exclude windows 7
            if (Environment.OSVersion.Version.Major > 6)
            {
                try
                {
                    toastOperation.Invoke();
                }
                catch (COMException) { }
                catch (InvalidCastException) { }
                catch (UnauthorizedAccessException) { }
                //seems un catchable
                catch (ArgumentException) { }
            }
        }
    }
}