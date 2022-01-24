using Microsoft.Win32;
using Snap.Exception;
using System.Diagnostics;

namespace DGP.Genshin.Helper
{
    public class AutoRun
    {
        private const string AppName = "SnapGenshin";
        private const string RunPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        public bool IsAutoRun
        {
            get
            {
                RegistryKey currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                RegistryKey? run = currentUser.OpenSubKey(RunPath);
                return run?.GetValue(AppName) is not null;
            }
            set
            {
                RegistryKey currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                RegistryKey? run = currentUser.CreateSubKey(RunPath);
                _ = run ?? throw new SnapGenshinInternalException("创建注册表项失败");

                if (value)
                {
                    string? appFileName = Process.GetCurrentProcess().MainModule?.FileName;
                    _ = appFileName ?? throw new SnapGenshinInternalException("无法找到主程序集");
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
