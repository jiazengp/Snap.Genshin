using DGP.Genshin.Core.Background.Abstraction;
using DGP.Genshin.Helper;
using DGP.Genshin.Helper.Extension;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Messaging;
using ModernWpf;
using ModernWpf.Media.Animation;
using Snap.Core.Logging;
using Snap.Exception;
using Snap.Extenion.Enumerable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace DGP.Genshin.Core.Background
{
    /// <summary>
    /// 背景图片加载器
    /// </summary>
    internal class BackgroundLoader
    {
        private const int animationDurationMilliseconds = 500;

        private readonly MainWindow mainWindow;
        private readonly IMessenger messenger;
        public IBackgroundProvider? BackgroundProvider { get; set; }

        public BackgroundLoader(MainWindow mainWindow, IMessenger messenger)
        {
            this.mainWindow = mainWindow;
            this.messenger = messenger;
        }

        private double Lightness { get; set; }

        public async Task LoadNextWallpaperAsync()
        {
            BackgroundProvider ??= new DefaultBackgroundProvider();
            BitmapImage? image = await BackgroundProvider.GetNextBitmapImageAsync();
            if(image == null)
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
                //Fade out old image
                DoubleAnimation fadeOutAnimation = AnimationHelper.CreateAnimation(0, animationDurationMilliseconds);
                fadeOutAnimation.EasingFunction = new CubicBezierEase { EasingMode = EasingMode.EaseOut };
                fadeOutAnimation.FillBehavior = FillBehavior.Stop;

                mainWindow.BackgroundGrid.Background.BeginAnimation(Brush.OpacityProperty, fadeOutAnimation);
                await Task.Delay(animationDurationMilliseconds);
                mainWindow.BackgroundGrid.Background.BeginAnimation(Brush.OpacityProperty, null);
                //Fade in new image
                mainWindow.BackgroundGrid.Background = new ImageBrush
                {
                    ImageSource = image,
                    Stretch = Stretch.UniformToFill,
                    Opacity = 0
                };

                DoubleAnimation fadeInAnimation = AnimationHelper.CreateAnimation(Setting2.BackgroundOpacity, animationDurationMilliseconds);
                fadeInAnimation.EasingFunction = new CubicBezierEase { EasingMode = EasingMode.EaseOut };
                fadeInAnimation.FillBehavior = FillBehavior.Stop;

                mainWindow.BackgroundGrid.Background.BeginAnimation(Brush.OpacityProperty, fadeInAnimation);
                await Task.Delay(animationDurationMilliseconds);
                mainWindow.BackgroundGrid.Background.BeginAnimation(Brush.OpacityProperty, null);
                mainWindow.BackgroundGrid.Background.Opacity = Setting2.BackgroundOpacity;

                messenger.Send(new Message.AdaptiveBackgroundOpacityChangedMessage(Setting2.BackgroundOpacity));
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

        internal class DefaultBackgroundProvider : IBackgroundProvider
        {
            private const string BackgroundFolder = "Background";

            private readonly List<string> supportedFiles;
            private static readonly IEnumerable<string> supportedExtensions =
                new List<string>() { ".png", ".jpg", ".jpeg", ".bmp" };

            public DefaultBackgroundProvider()
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
                if (supportedFiles.GetRandom() is string randomPath)
                {
                    this.Log($"Loading background wallpaper from {randomPath}");
                    return GetBitmapImageFromPath(randomPath);
                }
                return null;
            }
            private BitmapImage GetBitmapImageFromPath(string randomPath)
            {
                BitmapImage image;
                try
                {
                    image = new(new(randomPath));
                }
                catch
                {
                    throw new SnapGenshinInternalException($"无法读取图片：{randomPath}");
                }

                return image;
            }
        }
    }
}
