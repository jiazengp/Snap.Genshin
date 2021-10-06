﻿using System;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DGP.Genshin.Controls.CachedImage
{
    public class ImageAsyncHelper : DependencyObject
    {
        public static string GetImageUrl(DependencyObject obj) { return (string)obj.GetValue(ImageUrlProperty); }
        public static void SetImageUrl(DependencyObject obj, Uri value) { obj.SetValue(ImageUrlProperty, value); }

        public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.RegisterAttached(
            "ImageUrl", typeof(string), typeof(ImageAsyncHelper), new PropertyMetadata
            {
                PropertyChangedCallback = async (obj, e) =>
                {
                    System.IO.MemoryStream memoryStream = await FileCache.HitAsync((string)e.NewValue);
                    if (memoryStream == null)
                    {
                        return;
                    }

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();

                    ((Border)obj).Background = new ImageBrush()
                    {
                        ImageSource = bitmapImage,
                        Stretch = GetStretchMode(obj)
                    };
                }
            });

        public static Stretch GetStretchMode(DependencyObject obj)
        {
            return (Stretch)obj.GetValue(StretchModeProperty);
        }

        public static void SetStretchMode(DependencyObject obj, Stretch value)
        {
            obj.SetValue(StretchModeProperty, value);
        }
        public static readonly DependencyProperty StretchModeProperty =
            DependencyProperty.RegisterAttached("StretchMode", typeof(Stretch), typeof(ImageAsyncHelper), new PropertyMetadata(Stretch.Uniform));
    }
}
