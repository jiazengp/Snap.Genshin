using DGP.Genshin.Services.Settings;
using DGP.Snap.Framework.Data.Behavior;
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
        public bool TryInitialize()
        {
            string launcherPath = SettingService.Instance.GetOrDefault<string>(Setting.LauncherPath, null);
            if (!File.Exists(launcherPath) || Path.GetFileNameWithoutExtension(launcherPath) != "config")
            {
                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    Filter = "启动器|launcher.exe",
                    Title = "导出至图片",
                    CheckPathExists = true,
                    FileName = $"launcher.exe"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    launcherPath = openFileDialog.FileName;
                }
                else
                {
                    return false;
                }
            }
            string launcherDir = Path.GetDirectoryName(launcherPath);

            this.configPath = $@"{launcherDir}\config.ini";
            this.fileSystemWatcher = new FileSystemWatcher(launcherDir);

            this.fileSystemWatcher.Created += OnGameScreenshotCreated;
            return true;
        }

        public void UnInitialize()
        {
            this.fileSystemWatcher.Created -= OnGameScreenshotCreated;
            this.fileSystemWatcher.Dispose();
        }

        private void OnGameScreenshotCreated(object sender, FileSystemEventArgs e) => this.ScreenShots.Add(new Screenshot(e.FullPath));

        public ObservableCollection<Screenshot> ScreenShots { get; set; } = new ObservableCollection<Screenshot>();
    }
    public class Screenshot
    {
        public Screenshot(string path)
        {
            this.Path = path;
        }
        public string Path { get; set; }
        public string Name { get; set; }
    }
}
