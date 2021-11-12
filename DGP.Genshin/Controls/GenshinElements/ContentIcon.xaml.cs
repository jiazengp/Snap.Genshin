using DGP.Genshin.Common.Data.Behavior;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DGP.Genshin.Controls.GenshinElements
{
    public partial class ContentIcon : Button
    {
        public ContentIcon()
        {
            //suppress the databinding warning
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
            Loaded += ContentIcon_Loaded;
            InitializeComponent();
        }

        private bool isFirstLoaded = true;
        private void ContentIcon_Loaded(object sender, RoutedEventArgs e)
        {
            if (isFirstLoaded)
            {
                isFirstLoaded = false;
                Storyboard? storyBoard = FindResource("FadeInAnimation") as Storyboard;
                if (storyBoard is not null)
                {
                    storyBoard.Begin();
                }
            }
        }

        public string BackgroundUrl
        {
            get => (string)GetValue(BackgroundUrlProperty);
            set => SetValue(BackgroundUrlProperty, value);
        }
        public static readonly DependencyProperty BackgroundUrlProperty =
            DependencyProperty.Register("BackgroundUrl", typeof(string), typeof(ContentIcon), new PropertyMetadata(null));

        public string ForegroundUrl
        {
            get => (string)GetValue(ForegroundUrlProperty);
            set => SetValue(ForegroundUrlProperty, value);
        }
        public static readonly DependencyProperty ForegroundUrlProperty =
            DependencyProperty.Register("ForegroundUrl", typeof(string), typeof(ContentIcon), new PropertyMetadata(null));

        public string BadgeUrl
        {
            get => (string)GetValue(BadgeUrlProperty);
            set => SetValue(BadgeUrlProperty, value);
        }
        public static readonly DependencyProperty BadgeUrlProperty =
            DependencyProperty.Register("BadgeUrl", typeof(string), typeof(ContentIcon), new PropertyMetadata(null));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ContentIcon), new PropertyMetadata(""));

        public bool IsCountVisible
        {
            get => (bool)GetValue(IsCountVisibleProperty);
            set => SetValue(IsCountVisibleProperty, value);
        }
        public static readonly DependencyProperty IsCountVisibleProperty =
            DependencyProperty.Register("IsCountVisible", typeof(bool), typeof(ContentIcon), new PropertyMetadata(BoxedValue.FalseBox));
    }
}
