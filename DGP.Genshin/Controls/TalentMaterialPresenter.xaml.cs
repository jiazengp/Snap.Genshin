using DGP.Genshin.Data.Talent;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls
{
    /// <summary>
    /// TalentMaterialPresenter.xaml 的交互逻辑
    /// </summary>
    public partial class TalentMaterialPresenter : UserControl
    {
        public TalentMaterialPresenter()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        public TalentMaterial TalentMaterial
        {
            get => (TalentMaterial)this.GetValue(TalentMaterialProperty);
            set => this.SetValue(TalentMaterialProperty, value);
        }
        public static readonly DependencyProperty TalentMaterialProperty =
            DependencyProperty.Register("TalentMaterial", typeof(TalentMaterial), typeof(TalentMaterialPresenter), new PropertyMetadata(TalentMaterial.Ballad));

    }
}
