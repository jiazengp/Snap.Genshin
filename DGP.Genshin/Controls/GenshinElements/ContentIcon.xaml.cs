using DGP.Snap.Framework.Data.Behavior;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls.GenshinElements
{
    public partial class ContentIcon : Button
    {
        public ContentIcon()
        {
            //suppress the databinding warning
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
            this.InitializeComponent();
        }

        public string BackgroundUrl
        {
            get => (string)this.GetValue(BackgroundUrlProperty);
            set => this.SetValue(BackgroundUrlProperty, value);
        }
        public static readonly DependencyProperty BackgroundUrlProperty =
            DependencyProperty.Register("BackgroundUrl", typeof(string), typeof(ContentIcon), new PropertyMetadata(null));

        public string ForegroundUrl
        {
            get => (string)this.GetValue(ForegroundUrlProperty);
            set => this.SetValue(ForegroundUrlProperty, value);
        }
        public static readonly DependencyProperty ForegroundUrlProperty =
            DependencyProperty.Register("ForegroundUrl", typeof(string), typeof(ContentIcon), new PropertyMetadata(null));

        public string BadgeUrl
        {
            get => (string)this.GetValue(BadgeUrlProperty);
            set => this.SetValue(BadgeUrlProperty, value);
        }
        public static readonly DependencyProperty BadgeUrlProperty =
            DependencyProperty.Register("BadgeUrl", typeof(string), typeof(ContentIcon), new PropertyMetadata(null));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ContentIcon), new PropertyMetadata(""));

        public bool IsCountVisible
        {
            get => (bool)this.GetValue(IsCountVisibleProperty);
            set => this.SetValue(IsCountVisibleProperty, value);
        }
        [SuppressMessage("", "CA1416")]
        public static readonly DependencyProperty IsCountVisibleProperty =
            DependencyProperty.Register("IsCountVisible", typeof(bool), typeof(ContentIcon), new PropertyMetadata(BoxedValue.FalseBox));
    }
}
