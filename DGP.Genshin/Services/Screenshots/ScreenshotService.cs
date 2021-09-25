using DGP.Genshin.Services.Settings;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Device.Keyboard;
using DGP.Snap.Framework.Extensions.System;
using DGP.Snap.Framework.Extensions.System.Windows.Threading;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DGP.Genshin.Services.Screenshots
{
    /// <summary>
    /// TODO add keyboard hook
    /// </summary>
    public class ScreenshotService : Observable
    {
        private const string ScreenshotFolder = "Screenshots";
        private FileSystemWatcher gameScreenshotWatcher;
        private FileSystemWatcher localScreenshotWatcher;
        private GlobalKeyboardHook globalKeyboardHook;

        #region LifeCycle
        private bool isInitialized = false;
        public void Initialize()
        {
            if (this.isInitialized)
            {
                return;
            }
            else
            {
                this.isInitialized = true;
            }

            Directory.CreateDirectory(ScreenshotFolder);

            foreach (string image in Directory.EnumerateFiles($@"{Directory.GetCurrentDirectory()}\{ScreenshotFolder}"))
            {
                this.Screenshots.Add(new Screenshot(image));
            }
            string launcherPath = SettingService.Instance.GetOrDefault<string>(Setting.LauncherPath, null);
            string launcherDir = Path.GetDirectoryName(launcherPath);
            foreach (string image in Directory.EnumerateFiles($@"{launcherDir}\Genshin Impact Game\ScreenShot"))
            {
                string filename = $@"{Directory.GetCurrentDirectory()}\{ScreenshotFolder}\{DateTime.Now:yyyy-MM-dd HH-mm-ss}.{Guid.NewGuid()}.png";
                File.Move(image, filename);
                this.Screenshots.Add(new Screenshot(filename));
            }
            string gamePath = $@"{launcherDir}\Genshin Impact Game\ScreenShot";
            this.gameScreenshotWatcher = new FileSystemWatcher(gamePath)
            {
                EnableRaisingEvents = true
            };
            this.gameScreenshotWatcher.Created += OnGameScreenshotCreated;

            string localPath = $@"{Directory.GetCurrentDirectory()}\{ScreenshotFolder}";
            this.localScreenshotWatcher = new FileSystemWatcher(localPath)
            {
                EnableRaisingEvents = true
            };
            this.localScreenshotWatcher.Created += OnScreenshotCreated;
            this.localScreenshotWatcher.Deleted += OnLocalScreenshotDeleted;

            SetupKeyboardHooks();
            this.Log("initialized");
        }

        /// <summary>
        /// move game generated screenshot to out folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameScreenshotCreated(object sender, FileSystemEventArgs e) => File.Move(e.FullPath, $@"{ScreenshotFolder}\{DateTime.Now:yyyy-MM-dd HH-mm-ss.fffffff}.png");

        public void SetupKeyboardHooks()
        {
            this.globalKeyboardHook = new GlobalKeyboardHook();
            this.globalKeyboardHook.KeyboardPressed += OnKeyPressed;
        }
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
            this.globalKeyboardHook?.Dispose();
            this.Log("uninitialized");
        }
        #endregion

        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            if (e.KeyboardData.VirtualCode != GlobalKeyboardHook.VkSnapshot)
            {
                return;
            }
            if (e.KeyboardState == KeyboardState.KeyDown)
            {
                this.Log("PrtScr Pressed");
                TakeFullScreenshot();
                e.Handled = true;
            }
        }
        private void TakeFullScreenshot()
        {
            double screenLeft = SystemParameters.VirtualScreenLeft;
            double screenTop = SystemParameters.VirtualScreenTop;
            double screenWidth = SystemParameters.VirtualScreenWidth;
            double screenHeight = SystemParameters.VirtualScreenHeight;
            try
            {
                using (Bitmap bmp = new Bitmap((int)screenWidth, (int)screenHeight))
                {
                    using (Graphics graphics = Graphics.FromImage(bmp))
                    {
                        string filename = $@"{ScreenshotFolder}\{DateTime.Now:yyyy-MM-dd HH-mm-ss.fffffff}.png";
                        graphics.CopyFromScreen((int)screenLeft, (int)screenTop, 0, 0, bmp.Size);
                        bmp.Save(filename, ImageFormat.Png);
                    }
                }
                this.Log("take and save screenshot complete");
            }
            catch
            {
                //save img failed
            }
        }

        private void OnScreenshotCreated(object sender, FileSystemEventArgs e)
        {
            //avoid IOException from some strange issue
            Thread.Sleep(1000);
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
