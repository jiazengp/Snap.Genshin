using System;
using System.Runtime.InteropServices;

namespace DGP.Genshin.Helper.Notification
{
    internal class SecureToastNotificationContext
    {
        internal static void TryCatch(Action toastOperation)
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