using DGP.Genshin.Data.Character;
using ModernWpf.Controls;
using System.Windows;

namespace DGP.Genshin.Controls
{
    /// <summary>
    /// CharacterDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CharacterDialog : ContentDialog
    {
        public CharacterDialog()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }
        public CharacterDialog(Character c) : this()
        {
            this.Character = c;
        }
        public CharacterDialog(CharacterIcon ci) : this(ci.Character) { }
        public Character Character
        {
            get => (Character)this.GetValue(CharacterProperty);
            set => this.SetValue(CharacterProperty, value);
        }
        public static readonly DependencyProperty CharacterProperty =
            DependencyProperty.Register("Character", typeof(Character), typeof(CharacterDialog), new PropertyMetadata(null));
    }
}
