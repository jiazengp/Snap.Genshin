using Microsoft.Win32;
using System.Diagnostics;

namespace DGP.Genshin.Helpers
{
    public class AutoRunHelper
    {
        private const string AppName = "SnapGenshin";
        private const string RunPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public bool IsAutoRun
        {
            get
            {
                RegistryKey currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                RegistryKey run = currentUser.OpenSubKey(RunPath);
                return run != null && run.GetValue(AppName) != null;
            }
            set
            {
                RegistryKey currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                RegistryKey run = currentUser.OpenSubKey(RunPath, true);
                if (run == null)
                {
                    currentUser.CreateSubKey(RunPath);
                    run = currentUser.OpenSubKey(RunPath, true);
                }

                if (value)
                {
                    run.SetValue(AppName, Process.GetCurrentProcess().MainModule.FileName);
                }
                else
                {
                    run.DeleteValue(AppName);
                }
            }
        }
    }
}
