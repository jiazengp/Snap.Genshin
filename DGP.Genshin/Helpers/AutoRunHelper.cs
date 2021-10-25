using DGP.Genshin.Common.Exceptions;
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
                RegistryKey? run = currentUser.OpenSubKey(RunPath);
                return run is not null && run.GetValue(AppName) is not null;
            }
            set
            {
                RegistryKey currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                RegistryKey? run = currentUser.CreateSubKey(RunPath);

                if (run is null)
                {
                    throw new ExtremelyUnlikelyException("创建注册表项失败");
                }

                if (value)
                {
                    string? appFileName = Process.GetCurrentProcess().MainModule?.FileName;
                    Debug.Assert(appFileName is not null);
                    run.SetValue(AppName, appFileName);
                }
                else
                {
                    run.DeleteValue(AppName);
                }

            }
        }
    }
}
