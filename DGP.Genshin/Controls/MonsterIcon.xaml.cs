using DGP.Genshin.Data.Monster;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls
{
    /// <summary>
    /// MonsterIcon.xaml 的交互逻辑
    /// </summary>
    public partial class MonsterIcon : UserControl
    {
        public MonsterIcon()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        public Monster Monster
        {
            get => (Monster)this.GetValue(MonsterProperty);
            set => this.SetValue(MonsterProperty, value);
        }
        public static readonly DependencyProperty MonsterProperty =
            DependencyProperty.Register("Monster", typeof(Monster), typeof(MonsterIcon), new PropertyMetadata(null));
    }
}
