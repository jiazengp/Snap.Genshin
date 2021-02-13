using DGP.Genshin.Data.Character;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls
{
    /// <summary>
    /// CharacterIcon.xaml 的交互逻辑
    /// </summary>
    public partial class CharacterIcon : UserControl
    {
        public CharacterIcon()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        public Character Character
        {
            get => (Character)this.GetValue(CharacterProperty);
            set => this.SetValue(CharacterProperty, value);
        }
        public static readonly DependencyProperty CharacterProperty =
            DependencyProperty.Register("Character", typeof(Character), typeof(CharacterIcon), new PropertyMetadata(null));

        private void IconClick(object sender, RoutedEventArgs e)
        {
            new CharacterDialog(Character).ShowAsync();
        }
    }
}
