using DGP.Genshin.Data.Characters;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls.Characters
{
    /// <summary>
    /// CharacterItem.xaml 的交互逻辑
    /// </summary>
    public partial class CharacterItem : UserControl
    {
        public CharacterItem()
        {
            DataContext = this;
            this.InitializeComponent();
        }

        public Character Character
        {
            get { return (Character)GetValue(CharacterProperty); }
            set { SetValue(CharacterProperty, value); }
        }
        public static readonly DependencyProperty CharacterProperty =
            DependencyProperty.Register("Character", typeof(Character), typeof(CharacterItem), new PropertyMetadata(null));

    }
}
