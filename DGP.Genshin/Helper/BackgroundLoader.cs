using DGP.Genshin.Helper.Extension;
using DGP.Genshin.Service.Abstraction;
using ModernWpf;
using Snap.Core.Logging;
using Snap.Exception;
using Snap.Extenion.Enumerable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DGP.Genshin.Helper
{
    /// <summary>
    /// TODO: impl change opacity while switch app theme
    /// </summary>
    internal class BackgroundLoader
    {
        private const string BackgroundFolder = "Background";
        private readonly IEnumerable<string> supportedExtensions =
            new List<string>() { ".png", ".jpg", ".jpeg", ".bmp" };

        private readonly MainWindow mainWindow;

        public BackgroundLoader(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public double Lightness { get; set; }

        public void LoadWallpaper()
        {
            string folder = PathContext.Locate(BackgroundFolder);
            Directory.CreateDirectory(folder);
            List<string> supportedFiles = Directory.EnumerateFiles(folder)
                .Where(path => supportedExtensions.Contains(Path.GetExtension(path)))
                .ToList();

            if (supportedFiles.GetRandom() is string randomPath)
            {
                this.Log($"Loading background wallpaper from {randomPath}");
                BitmapImage image;
                try
                {
                    image = new(new(randomPath));
                }
                catch
                {
                    throw new SnapGenshinInternalException($"无法读取图片：{randomPath}");
                }
                
                Pixel[,] pixels = image.GetPixels();
                Lightness = pixels
                    .Cast<Pixel>()
                    .Select(p => (p.Red * 0.299 + p.Green * 0.587 + p.Blue * 0.114) * (p.Alpha / 255D) / 255)
                    .Average();
                this.Log($"Lightness: {Lightness}");
                SetAdaptiveBackgroundOpacityValue();
                mainWindow.BackgroundGrid.Background = new ImageBrush
                {
                    ImageSource = image,
                    Stretch = Stretch.UniformToFill,
                    Opacity = Setting2.BackgroundOpacity.Get()
                };
            }
        }

        private void SetAdaptiveBackgroundOpacityValue()
        {
            if (Setting2.IsBackgroundOpacityAdaptive.Get())
            {
                bool isDarkMode = ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark;
                double targetOpacity = isDarkMode ? (1 - Lightness) * 0.4 : Lightness * 0.6;
                this.Log($"Adjust BackgroundOpacity to {targetOpacity}");
                Setting2.BackgroundOpacity.Set(targetOpacity);
            }
        }
    }
}
