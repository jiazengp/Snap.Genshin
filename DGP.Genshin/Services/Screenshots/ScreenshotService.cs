using DGP.Genshin.Services.Settings;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Extensions.System;
using DGP.Snap.Framework.Extensions.System.Windows.Threading;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace DGP.Genshin.Services.Screenshots
{
    /// <summary>
    /// keyboard hook is not practical
    /// cause game already hook the key
    /// </summary>
    public class ScreenshotService : Observable
    {
        private const string ScreenshotFolder = "Screenshots";
        private FileSystemWatcher gameScreenshotWatcher;
        private FileSystemWatcher localScreenshotWatcher;

        #region LifeCycle
        private bool isInitialized = false;
        public void Initialize()
        {
            if (this.isInitialized)
            {
                return;
            }
            this.isInitialized = true;

            Directory.CreateDirectory(ScreenshotFolder);

            //move launcher image to local
            string launcherPath = SettingService.Instance.GetOrDefault<string>(Setting.LauncherPath, null);
            string launcherDir = Path.GetDirectoryName(launcherPath);
            foreach (string image in Directory.EnumerateFiles($@"{launcherDir}\Genshin Impact Game\ScreenShot"))
            {
                string filename = $@"{Directory.GetCurrentDirectory()}\{ScreenshotFolder}\{DateTime.Now:yyyy-MM-dd HH-mm-ss}.{Guid.NewGuid()}.png";
                File.Move(image, filename);
            }
            //load local 
            foreach (string image in Directory.EnumerateFiles($@"{Directory.GetCurrentDirectory()}\{ScreenshotFolder}"))
            {
                this.Screenshots.Add(new Screenshot(image));
            }

            //watch game folder
            string gamePath = $@"{launcherDir}\Genshin Impact Game\ScreenShot";
            this.gameScreenshotWatcher = new FileSystemWatcher(gamePath)
            {
                EnableRaisingEvents = true
            };
            this.gameScreenshotWatcher.Created += OnGameScreenshotCreated;
            //watch local folder
            string localPath = $@"{Directory.GetCurrentDirectory()}\{ScreenshotFolder}";
            this.localScreenshotWatcher = new FileSystemWatcher(localPath)
            {
                EnableRaisingEvents = true
            };
            this.localScreenshotWatcher.Created += OnScreenshotCreated;
            this.localScreenshotWatcher.Deleted += OnLocalScreenshotDeleted;

            this.Log("initialized");
        }

        /// <summary>
        /// move game generated screenshot to our folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameScreenshotCreated(object sender, FileSystemEventArgs e) =>
            File.Move(e.FullPath, $@"{ScreenshotFolder}\{DateTime.Now:yyyy-MM-dd HH-mm-ss.fffffff}.png");

        public void UnInitialize()
        {
            if (this.gameScreenshotWatcher != null)
            {
                this.gameScreenshotWatcher.Created -= OnScreenshotCreated;
                this.gameScreenshotWatcher.Dispose();
            }
            if (this.localScreenshotWatcher != null)
            {
                this.localScreenshotWatcher.Created -= OnScreenshotCreated;
                this.localScreenshotWatcher.Dispose();
            }
            this.Log("uninitialized");
        }
        #endregion


        private void OnScreenshotCreated(object sender, FileSystemEventArgs e)
        {
            //avoid IOException from some strange issue
            Thread.Sleep(500);
            App.Current.Invoke(() => this.Screenshots.Add(new Screenshot(e.FullPath)));
        }

        private void OnLocalScreenshotDeleted(object sender, FileSystemEventArgs e)
        {
            Screenshot item = this.Screenshots.First(i => i.Path == e.FullPath);
            if (item != null)
            {
                App.Current.Invoke(() => this.Screenshots.Remove(item));
            }
        }

        public ObservableCollection<Screenshot> Screenshots { get; set; } =
            new ObservableCollection<Screenshot>();

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
