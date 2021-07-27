using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DGP.Genshin.Controls.CachedImage
{
    /// <summary>
    ///     Represents a control that is a wrapper on System.Windows.Controls.Image for enabling filesystem-based caching
    /// </summary>
    public class CachedImage : System.Windows.Controls.Image
    {
        static CachedImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CachedImage),
                new FrameworkPropertyMetadata(typeof(CachedImage)));
        }

        public string ImageUrl
        {
            get => (string)this.GetValue(ImageUrlProperty);
            set => this.SetValue(ImageUrlProperty, value);
        }

        public BitmapCreateOptions CreateOptions
        {
            get => (BitmapCreateOptions)this.GetValue(CreateOptionsProperty);
            set => this.SetValue(CreateOptionsProperty, value);
        }

        private static async void ImageUrlPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            string url = e.NewValue as string;

            if (String.IsNullOrEmpty(url))
                return;

            CachedImage cachedImage = (CachedImage)obj;
            BitmapImage bitmapImage = new BitmapImage();

            try
            {
                System.IO.MemoryStream memoryStream = await FileCache.HitAsync(url);
                if (memoryStream == null)
                    return;

                bitmapImage.BeginInit();
                bitmapImage.CreateOptions = cachedImage.CreateOptions;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                cachedImage.Source = bitmapImage;
            }
            catch (Exception)
            {
                // ignored, in case the downloaded file is a broken or not an image.
            }
        }

        public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.Register("ImageUrl",
            typeof(string), typeof(CachedImage), new PropertyMetadata("", ImageUrlPropertyChanged));

        public static readonly DependencyProperty CreateOptionsProperty = DependencyProperty.Register("CreateOptions",
            typeof(BitmapCreateOptions), typeof(CachedImage));
    }
}
