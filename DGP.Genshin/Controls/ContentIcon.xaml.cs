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

        public string BackgroundUrl
        {
            get { return (string)GetValue(BackgroundUrlProperty); }
            set { SetValue(BackgroundUrlProperty, value); }
        }
        public static readonly DependencyProperty BackgroundUrlProperty =
            DependencyProperty.Register("BackgroundUrl", typeof(string), typeof(ContentIcon), new PropertyMetadata(null));

        public string ForegroundUrl
        {
            get { return (string)GetValue(ForegroundUrlProperty); }
            set { SetValue(ForegroundUrlProperty, value); }
        }
        public static readonly DependencyProperty ForegroundUrlProperty =
            DependencyProperty.Register("ForegroundUrl", typeof(string), typeof(ContentIcon), new PropertyMetadata(null));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ContentIcon), new PropertyMetadata(""));
    }
}
