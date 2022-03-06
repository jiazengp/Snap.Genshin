using Snap.Data.Primitive;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DGP.Genshin.Control.GenshinElement
{
    public sealed partial class ContentIcon : Button
    {
        public ContentIcon()
        {
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
            Loaded += ContentIcon_Loaded;
            InitializeComponent();
        }

        private const string FadeInAnimationKey = "FadeInAnimation";

        private void ContentIcon_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard? storyBoard = FindResource(FadeInAnimationKey) as Storyboard;
            storyBoard?.Begin();
            //thus only affect first load
            Loaded -= ContentIcon_Loaded;
        }

        public string BackgroundUrl
        {
            get => (string)GetValue(BackgroundUrlProperty);

            set => SetValue(BackgroundUrlProperty, value);
        }
        public static readonly DependencyProperty BackgroundUrlProperty =
            DependencyProperty.Register(nameof(BackgroundUrl), typeof(string), typeof(ContentIcon));

        public string ForegroundUrl
        {
            get => (string)GetValue(ForegroundUrlProperty);

            set => SetValue(ForegroundUrlProperty, value);
        }
        public static readonly DependencyProperty ForegroundUrlProperty =
            DependencyProperty.Register(nameof(ForegroundUrl), typeof(string), typeof(ContentIcon));

        public string BadgeUrl
        {
            get => (string)GetValue(BadgeUrlProperty);

            set => SetValue(BadgeUrlProperty, value);
        }
        public static readonly DependencyProperty BadgeUrlProperty =
            DependencyProperty.Register(nameof(BadgeUrl), typeof(string), typeof(ContentIcon));

        public string Text
        {
            get => (string)GetValue(TextProperty);

            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(ContentIcon), new PropertyMetadata(""));

        public bool IsCountVisible
        {
            get => (bool)GetValue(IsCountVisibleProperty);

            set => SetValue(IsCountVisibleProperty, value);
        }
        public static readonly DependencyProperty IsCountVisibleProperty =
            DependencyProperty.Register(nameof(IsCountVisible), typeof(bool), typeof(ContentIcon), new PropertyMetadata(BoxedValue.FalseBox));
    }
}
