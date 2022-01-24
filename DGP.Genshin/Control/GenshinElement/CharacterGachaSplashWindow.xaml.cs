using System;
using System.ComponentModel;
using System.Windows;

namespace DGP.Genshin.Control.GenshinElement
{
    /// <summary>
    /// CharacterGachaSplashWindwo.xaml 的交互逻辑
    /// </summary>
    public partial class CharacterGachaSplashWindow : Window
    {
        public CharacterGachaSplashWindow()
        {
            DataContext = this;
            InitializeComponent();
        }
        public string? Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(string), typeof(CharacterGachaSplashWindow));

        private bool isAlreadyClosed = false;

        protected override void OnClosing(CancelEventArgs e)
        {
            isAlreadyClosed = true;
            base.OnClosing(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            if (!isAlreadyClosed)
            {
                DialogResult = true;
            }
        }
    }
}
