using DGP.Genshin.Controls.Simulations;
using DGP.Genshin.Services;
using System.Windows.Navigation;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// CalculationsPage.xaml 的交互逻辑
    /// </summary>
    public partial class CalculationsPage : ModernWpf.Controls.Page
    {
        private SimulationService Service { get; set; }
        public CalculationsPage()
        {
            this.Service = new SimulationService();
            this.DataContext = this.Service;
            this.Service.Initialize();
            this.InitializeComponent();
        }

        private async void AddSimulationCollectionButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            SimulationCollectionDialog dialog = new SimulationCollectionDialog();
            ModernWpf.Controls.ContentDialogResult result = await dialog.ShowAsync();
            if (result == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                this.Service.SimulationCollections.Add(new Data.Simulations.SimulationCollection { Name = dialog.InputName, Description = dialog.InputDescription });
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.Service.UnInitialize();
            base.OnNavigatedFrom(e);
        }

        private void OnDeleteItem(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Service.SimulationCollections.Remove(this.Service.SelectedSimulationCollection);
            this.Service.SelectedSimulationCollection = null;
        }

        private void AddSimulationButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            //Service.SelectedSimulationCollection
        }
    }
}
