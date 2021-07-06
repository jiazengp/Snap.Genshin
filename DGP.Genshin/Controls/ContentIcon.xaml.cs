using DGP.Genshin.Controls.CachedImage;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DGP.Genshin.Controls
{
    public partial class ContentIcon : Button
    {
        public ContentIcon()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        private async Task<ImageSource> UrlToImageSourceAsync(string url)
        {

            if (String.IsNullOrEmpty(url))
                return null;
                //return await Task.FromResult<ImageSource>(null);

            BitmapImage bitmapImage = new BitmapImage();

            try
            {
                System.IO.MemoryStream memoryStream = await FileCache.HitAsync(url);
                if (memoryStream == null)
                    return null;

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch (Exception)
            {
                // ignored, in case the downloaded file is a broken or not an image.
                return null;
            }
        }

        public ImageSource RankBackground
        {
            get => (ImageSource)this.GetValue(StarProperty);
            set => this.SetValue(StarProperty, value);
        }
        public static readonly DependencyProperty StarProperty =
            DependencyProperty.Register("RankBackground", typeof(ImageSource), typeof(ContentIcon), new PropertyMetadata(null));

        public ImageSource Source
        {
            get => (ImageSource)this.GetValue(SourceProperty);
            set => this.SetValue(SourceProperty, value);
        }
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(ContentIcon), new PropertyMetadata(null));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ContentIcon), new PropertyMetadata(""));
    }
}
