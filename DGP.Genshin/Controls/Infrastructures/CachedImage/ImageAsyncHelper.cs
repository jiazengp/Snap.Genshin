using DGP.Genshin.Messages;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DGP.Genshin.Controls.Infrastructures.CachedImage
{
    /// <summary>
    /// 实现了图片缓存，
    /// 用来在 <see cref="Border"/> 上设置异步设置 <see cref="Border.Background"/> 与相应的 <see cref="ImageBrush.Stretch"/>，
    /// 因为只有 <see cref="Border"/> 可以较为便捷的设置角落弧度 <see cref="Border.CornerRadius"/>
    /// </summary>
    public class ImageAsyncHelper
    {
        #region ImageUrl
        public static string GetImageUrl(Border obj)
        {
            return (string)obj.GetValue(ImageUrlProperty);
        }

        public static void SetImageUrl(Border obj, Uri value)
        {
            obj.SetValue(ImageUrlProperty, value);
        }

        private static int imageUrlHittingCount = 0;

        public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.RegisterAttached(
            "ImageUrl", typeof(string), typeof(ImageAsyncHelper), new PropertyMetadata
            {
                PropertyChangedCallback = async (obj, e) =>
                {
                    MemoryStream? memoryStream;
                    if (FileCache.Exists((string)e.NewValue))
                    {
                        memoryStream = await FileCache.HitAsync((string)e.NewValue);
                    }
                    else
                    {
                        if (imageUrlHittingCount == 0)
                        {
                            App.Messenger.Send(new ImageHitBeginMessage());
                        }
                        Interlocked.Increment(ref imageUrlHittingCount);
                        memoryStream = await FileCache.HitAsync((string)e.NewValue);
                        Interlocked.Decrement(ref imageUrlHittingCount);
                        if (imageUrlHittingCount == 0)
                        {
                            App.Messenger.Send(new ImageHitEndMessage());
                        }
                    }

                    if (memoryStream == null)
                    {
                        return;
                    }

                    BitmapImage bitmapImage = new();
                    try
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();
                    }
                    catch { }

                    ((Border)obj).Background = new ImageBrush()
                    {
                        ImageSource = bitmapImage,
                        Stretch = GetStretchMode((Border)obj)
                    };
                }
            });
        #endregion

        #region StretchMode
        public static Stretch GetStretchMode(Border obj)
        {
            return (Stretch)obj.GetValue(StretchModeProperty);
        }

        public static void SetStretchMode(Border obj, Stretch value)
        {
            obj.SetValue(StretchModeProperty, value);
        }

        public static readonly DependencyProperty StretchModeProperty = DependencyProperty.RegisterAttached(
            "StretchMode", typeof(Stretch), typeof(ImageAsyncHelper), new PropertyMetadata(Stretch.Uniform));
        #endregion
    }
}
