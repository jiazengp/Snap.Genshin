using DGP.Genshin.Services.Settings;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Device.Keyboard;
using DGP.Snap.Framework.Extensions.System;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;

namespace DGP.Genshin.Services.Screenshots
{
    /// <summary>
    /// TODO add keyboard hook
    /// </summary>
    public class ScreenshotService : Observable
    {
        private string configPath;
        private FileSystemWatcher fileSystemWatcher;
        private GlobalKeyboardHook globalKeyboardHook;

        public bool IsAvailable { get; private set; }
        public void TryInitialize(bool forceInput)
        {
            string launcherPath = SettingService.Instance.GetOrDefault<string>(Setting.LauncherPath, null);
            if ((!File.Exists(launcherPath) || Path.GetFileNameWithoutExtension(launcherPath) != "config") && forceInput)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    Filter = "启动器|launcher.exe",
                    Title = "选择启动器文件",
                    CheckPathExists = true,
                    FileName = $"launcher.exe"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    launcherPath = openFileDialog.FileName;
                }
                else
                {
                    IsAvailable = false;
                    return;
                }
            }
            string launcherDir = Path.GetDirectoryName(launcherPath);

            this.configPath = $@"{launcherDir}\config.ini";
            this.fileSystemWatcher = new FileSystemWatcher(launcherDir);

            this.fileSystemWatcher.Created += OnGameScreenshotCreated;
            SetupKeyboardHooks();
            IsAvailable = true;
        }

        public void SetupKeyboardHooks()
        {
            globalKeyboardHook = new GlobalKeyboardHook();
            globalKeyboardHook.KeyboardPressed += OnKeyPressed;
        }

        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            //Debug.WriteLine(e.KeyboardData.VirtualCode);

            if (e.KeyboardData.VirtualCode != GlobalKeyboardHook.VkSnapshot)
                return;

            // seems, not needed in the life.
            //if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyDown &&
            //    e.KeyboardData.Flags == GlobalKeyboardHook.LlkhfAltdown)
            //{
            //    MessageBox.Show("Alt + Print Screen");
            //    e.Handled = true;
            //}
            //else

            if (e.KeyboardState == KeyboardState.KeyDown)
            {
                this.Log("PrtScr Pressed");
                e.Handled = true;
            }
        }

        public void UnInitialize()
        {
            if (this.fileSystemWatcher != null)
            {
                this.fileSystemWatcher.Created -= OnGameScreenshotCreated;
                this.fileSystemWatcher.Dispose();
            }
            this.globalKeyboardHook?.Dispose();
        }

        private void OnGameScreenshotCreated(object sender, FileSystemEventArgs e) =>
            this.ScreenShots.Add(new Screenshot(e.FullPath));

        public ObservableCollection<Screenshot> ScreenShots { get; set; } = new ObservableCollection<Screenshot>();

        #region 单例
        private static ScreenshotService instance;
        private static readonly object _lock = new();
        private ScreenshotService()
        {
        }
        public static ScreenshotService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new ScreenshotService();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
