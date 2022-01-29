namespace DGP.Genshin.Sample.Plugin
{
    public partial class SamplePage : System.Windows.Controls.Page
    {
        public SamplePage()
        {
            DataContext = App.AutoWired<SampleViewModel>();
            InitializeComponent();
        }
    }
}
