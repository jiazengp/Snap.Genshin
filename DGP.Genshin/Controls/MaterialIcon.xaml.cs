using DGP.Genshin.Data;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls
{
    /// <summary>
    /// MaterialIcon.xaml 的交互逻辑
    /// </summary>
    public partial class MaterialIcon : UserControl
    {
        public MaterialIcon()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }
        public Material Material
        {
            get => (Material)this.GetValue(MaterialProperty);
            set => this.SetValue(MaterialProperty, value);
        }
        public static readonly DependencyProperty MaterialProperty =
            DependencyProperty.Register("Material", typeof(Material), typeof(MaterialIcon), new PropertyMetadata(null));
    }
}
