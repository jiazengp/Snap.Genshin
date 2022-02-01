using System;
using System.Runtime.InteropServices;

namespace DGP.Genshin.Helper.Notification
{
    internal class SecureToastNotificationContext
    {
        internal static void TryCatch(Action toastOperation)
        {
            try
            {
                toastOperation.Invoke();
            }
            catch (DllNotFoundException) { }
            catch (COMException) { }
            catch (InvalidCastException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}