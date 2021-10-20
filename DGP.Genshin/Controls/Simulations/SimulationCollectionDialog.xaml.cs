using ModernWpf.Controls;
using System.Windows;

namespace DGP.Genshin.Controls.Simulations
{
    /// <summary>
    /// SimulationCollectionDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SimulationCollectionDialog : ContentDialog
    {
        public SimulationCollectionDialog()
        {
            this.InitializeComponent();
        }
        public string InputName
        {
            get => (string)this.GetValue(InputNameProperty);
            set => this.SetValue(InputNameProperty, value);
        }
        public static readonly DependencyProperty InputNameProperty =
            DependencyProperty.Register("InputName", typeof(string), typeof(SimulationCollectionDialog), new PropertyMetadata(null));

        public string InputDescription
        {
            get => (string)this.GetValue(InputDescriptionProperty);
            set => this.SetValue(InputDescriptionProperty, value);
        }
        public static readonly DependencyProperty InputDescriptionProperty =
            DependencyProperty.Register("InputDescription", typeof(string), typeof(SimulationCollectionDialog), new PropertyMetadata(null));


    }
}
