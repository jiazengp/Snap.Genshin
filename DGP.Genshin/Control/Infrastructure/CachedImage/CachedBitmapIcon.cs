using Microsoft.VisualStudio.Threading;
using Snap.Data.Utility.Extension;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DGP.Genshin.Control.Infrastructure.CachedImage
{
    /// <summary>
    /// Represents an icon that uses a bitmap as its content.
    /// </summary>
    [SuppressMessage("", "SA1101")]
    [SuppressMessage("", "SA1124")]
    [SuppressMessage("", "SA1201")]
    [SuppressMessage("", "SA1202")]
    [SuppressMessage("", "SA1309")]
    [SuppressMessage("", "SA1413")]
    [SuppressMessage("", "SA1600")]
    public sealed class CachedBitmapIcon : CachedIconElementBase
    {
        static CachedBitmapIcon()
        {
            ForegroundProperty.OverrideMetadata(typeof(CachedBitmapIcon), new FrameworkPropertyMetadata(OnForegroundChanged));
        }

        /// <summary>
        /// Initializes a new instance of the CachedBitmapIcon class.
        /// </summary>
        public CachedBitmapIcon()
        {
        }

        #region UriSource

        /// <summary>
        /// Identifies the UriSource dependency property.
        /// </summary>
        public static readonly DependencyProperty UriSourceProperty =
            BitmapImage.UriSourceProperty.AddOwner(
                typeof(CachedBitmapIcon),
                new FrameworkPropertyMetadata(OnUriSourceChanged));

        /// <summary>
        /// Gets or sets the Uniform Resource Identifier (URI) of the bitmap to use as the
        /// icon content.
        /// </summary>
        /// <returns>The Uri of the bitmap to use as the icon content. The default is **null**.</returns>
        public Uri UriSource
        {
            get => (Uri)this.GetValue(UriSourceProperty);

            set => this.SetValue(UriSourceProperty, value);
        }

        private static void OnUriSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CachedBitmapIcon)d).ApplyUriSource();
        }

        #endregion

        #region ShowAsMonochrome

        /// <summary>
        /// Identifies the ShowAsMonochrome dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowAsMonochromeProperty =
            DependencyProperty.Register(
                nameof(ShowAsMonochrome),
                typeof(bool),
                typeof(CachedBitmapIcon),
                new PropertyMetadata(true, OnShowAsMonochromeChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether the bitmap is shown in a single color.
        /// </summary>
        /// <returns>
        /// **true** to show the bitmap in a single color; **false** to show the bitmap in
        /// full color. The default is **true.**
        /// </returns>
        public bool ShowAsMonochrome
        {
            get => (bool)this.GetValue(ShowAsMonochromeProperty);

            set => this.SetValue(ShowAsMonochromeProperty, value);
        }

        private static void OnShowAsMonochromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CachedBitmapIcon)d).ApplyShowAsMonochrome();
        }

        #endregion

        private protected override void InitializeChildren()
        {
            this._image = new CachedImage
            {
                Visibility = Visibility.Hidden
            };

            this._opacityMask = new ImageBrush();
            this._foreground = new Rectangle
            {
                OpacityMask = _opacityMask
            };

            this.ApplyForeground();
            this.ApplyUriSource();

            this.Children.Add(this._image);

            this.ApplyShowAsMonochrome();
        }

        private protected override void OnShouldInheritForegroundFromVisualParentChanged()
        {
            this.ApplyForeground();
        }

        private protected override void OnVisualParentForegroundPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (this.ShouldInheritForegroundFromVisualParent)
            {
                this.ApplyForeground();
            }
        }

        private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CachedBitmapIcon)d).ApplyForeground();
        }

        private void ApplyForeground()
        {
            if (this._foreground != null)
            {
                this._foreground.Fill = this.ShouldInheritForegroundFromVisualParent ? this.VisualParentForeground : this.Foreground;
            }
        }

        private void ApplyUriSource()
        {
            this.ApplyUriSourceAsync().Forget();
        }

        private async Task ApplyUriSourceAsync()
        {
            if (this._image != null && this._opacityMask != null)
            {
                Uri uriSource = this.UriSource;
                if (uriSource != null)
                {
                    MemoryStream? stream = await FileCache.HitAsync(uriSource.ToString());
                    if (stream is not null)
                    {
                        try
                        {
                            BitmapImage imageSource = new();
                            using (imageSource.AsDisposableInit())
                            {
                                // imageSource.CreateOptions = BitmapCreateOptions.None;
                                imageSource.StreamSource = stream;
                            }

                            this._image.Source = imageSource;
                            this._opacityMask.ImageSource = imageSource;
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    this._image.ClearValue(Image.SourceProperty);
                    this._opacityMask.ClearValue(ImageBrush.ImageSourceProperty);
                }
            }
        }

        private void ApplyShowAsMonochrome()
        {
            bool showAsMonochrome = this.ShowAsMonochrome;

            if (this._image != null)
            {
                this._image.Visibility = showAsMonochrome ? Visibility.Hidden : Visibility.Visible;
            }

            if (this._foreground != null)
            {
                if (showAsMonochrome)
                {
                    if (!this.Children.Contains(this._foreground))
                    {
                        this.Children.Add(this._foreground);
                    }
                }
                else
                {
                    this.Children.Remove(this._foreground);
                }
            }
        }

        private Image? _image;
        private Rectangle? _foreground;
        private ImageBrush? _opacityMask;
    }
}