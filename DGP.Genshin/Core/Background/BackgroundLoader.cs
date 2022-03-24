using DGP.Genshin.Core.Background.Abstraction;
using DGP.Genshin.Core.ImplementationSwitching;
using DGP.Genshin.Helper;
using DGP.Genshin.Helper.Extension;
using DGP.Genshin.Message;
using DGP.Genshin.Service.Abstraction.Setting;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.Threading;
using ModernWpf;
using ModernWpf.Media.Animation;
using Snap.Core.Logging;
using Snap.Data.Utility.Extension;
using Snap.Exception;
using Snap.Extenion.Enumerable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace DGP.Genshin.Core.Background
{
    /// <summary>
    /// 背景图片加载器
    /// </summary>
    internal class BackgroundLoader : IRecipient<BackgroundOpacityChangedMessage>, IRecipient<BackgroundChangeRequestMessage>
    {
        private const int animationDuration = 500;

        private readonly MainWindow mainWindow;
        private readonly IMessenger messenger;

        public BackgroundLoader(MainWindow mainWindow, IMessenger messenger)
        {
            this.mainWindow = mainWindow;
            this.messenger = messenger;
            messenger.RegisterAll(this);
        }
        ~BackgroundLoader()
        {
            messenger.UnregisterAll(this);
        }

        private double Lightness { get; set; }

        public async Task LoadNextWallpaperAsync()
        {
            IBackgroundProvider? backgroundProvider = App.Current.SwitchableImplementationManager
                .CurrentBackgroundProvider!.Factory.Value;
            BitmapImage? image = await backgroundProvider.GetNextBitmapImageAsync();
            if (image == null)
            {
                return;
            }
            TrySetTargetAdaptiveBackgroundOpacityValue(image);
            //first pic
            if (mainWindow.BackgroundGrid.Background is null)
            {
                //直接设置背景
                mainWindow.BackgroundGrid.Background = new ImageBrush
                {
                    ImageSource = image,
                    Stretch = Stretch.UniformToFill,
                    Opacity = Setting2.BackgroundOpacity
                };
            }
            else
            {
                Grid backgroundPresenter = mainWindow.BackgroundGrid;
                DoubleAnimation fadeOutAnimation = AnimationHelper.CreateAnimation<CubicBezierEase>(0, animationDuration);
                //Fade out old image
                backgroundPresenter.Background.BeginAnimation(Brush.OpacityProperty, fadeOutAnimation);
                await Task.Delay(animationDuration);
                backgroundPresenter.Background.BeginAnimation(Brush.OpacityProperty, null);

                backgroundPresenter.Background = new ImageBrush
                {
                    ImageSource = image,
                    Stretch = Stretch.UniformToFill,
                    Opacity = 0
                };
                
                DoubleAnimation fadeInAnimation = AnimationHelper.CreateAnimation<CubicBezierEase>(Setting2.BackgroundOpacity, animationDuration);
                //Fade in new image
                backgroundPresenter.Background.BeginAnimation(Brush.OpacityProperty, fadeInAnimation);
                await Task.Delay(animationDuration);
                backgroundPresenter.Background.BeginAnimation(Brush.OpacityProperty, null);

                backgroundPresenter.Background.Opacity = Setting2.BackgroundOpacity;
                messenger.Send(new AdaptiveBackgroundOpacityChangedMessage(Setting2.BackgroundOpacity));
            }
        }

        /// <summary>
        /// TODO: remove heavy work that blocks UI thread.
        /// </summary>
        /// <param name="image"></param>
        private void TrySetTargetAdaptiveBackgroundOpacityValue(BitmapImage image)
        {
            if (Setting2.IsBackgroundOpacityAdaptive)
            {
                //this operation is really laggy
                Lightness = image.GetPixels()
                    .Cast<Pixel>()
                    .Select(p => (p.Red * 0.299 + p.Green * 0.587 + p.Blue * 0.114) * (p.Alpha / 255D) / 255)
                    .Average();

                this.Log($"Lightness: {Lightness}");

                bool isDarkMode = ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark;
                double targetOpacity = isDarkMode ? (1 - Lightness) * 0.4 : Lightness * 0.6;
                this.Log($"Adjust BackgroundOpacity to {targetOpacity}");
                Setting2.BackgroundOpacity.Set(targetOpacity);
            }
        }

        public void Receive(BackgroundOpacityChangedMessage message)
        {
            if (mainWindow.BackgroundGrid.Background is ImageBrush brush)
            {
                brush.Opacity = message.Value;
            }
        }
        public void Receive(BackgroundChangeRequestMessage message)
        {
            LoadNextWallpaperAsync().Forget();
        }

        [SwitchableImplementation(typeof(IBackgroundProvider))]
        internal class DefaultBackgroundProvider : IBackgroundProvider
        {
            private const string BackgroundFolder = "Background";

            private static readonly List<string> supportedFiles;
            private static readonly IEnumerable<string> supportedExtensions =
                new List<string>() { ".png", ".jpg", ".jpeg", ".bmp" };
            private static string? latestFile;

            static DefaultBackgroundProvider()
            {
                string folder = PathContext.Locate(BackgroundFolder);
                Directory.CreateDirectory(folder);
                supportedFiles = Directory.EnumerateFiles(folder)
                    .Where(path => supportedExtensions.Contains(Path.GetExtension(path).ToLowerInvariant()))
                    .ToList();
            }

            public async Task<BitmapImage?> GetNextBitmapImageAsync()
            {
                await Task.Yield();
                if (supportedFiles.GetRandomNoRepeat(latestFile) is string randomPath)
                {
                    latestFile = randomPath;
                    this.Log($"Loading background wallpaper from {randomPath}");
                    return GetBitmapImageFromPath(randomPath);
                }
                return null;
            }
            private BitmapImage GetBitmapImageFromPath(string randomPath)
            {
                BitmapImage image = new();
                try
                {
                    using (FileStream stream = new(randomPath, FileMode.Open))
                    {
                        using (image.AsDisposableInit())
                        {
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.StreamSource = stream;
                        }
                    }
                }
                catch
                {
                    throw new SnapGenshinInternalException($"无法读取图片：{randomPath}");
                }

                return image;
            }
        }

        [SwitchableImplementation(typeof(IBackgroundProvider), "Xunkong.Wallpaper", "寻空壁纸 实现")]
        internal class XunkongWallpaperProvider : IBackgroundProvider
        {
            private const string Api = "https://api.xunkong.cc/v0.1/genshin/wallpaper/redirect/random";
            // HttpClient is intended to be instantiated once per application, rather than per-use.
            private static readonly Lazy<HttpClient> LazyHttpClient = new(() => new() { Timeout = Timeout.InfiniteTimeSpan });

            public async Task<BitmapImage?> GetNextBitmapImageAsync()
            {
                try
                {
                    using (HttpResponseMessage resp = await LazyHttpClient.Value.GetAsync(Api, HttpCompletionOption.ResponseContentRead))
                    {
                        Stream result = await resp.Content.ReadAsStreamAsync();
                        BitmapImage bitmapImage = new();
                        using (bitmapImage.AsDisposableInit())
                        {
                            bitmapImage.StreamSource = result;
                        }
                        return bitmapImage;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
