using DGP.Genshin.ViewModel;
using ModernWpf.Controls;
using WPFUI.Controls;

namespace DGP.Genshin.Page
{
    /// <summary>
    /// RecordPage.xaml 的交互逻辑
    /// </summary>
    public partial class RecordPage : ModernWpf.Controls.Page
    {
        public RecordPage()
        {
            DataContext = App.GetViewModel<RecordViewModel>();
            InitializeComponent();
        }

        private RecordViewModel ViewModel => (RecordViewModel)DataContext;

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string? uid = args.ChosenSuggestion != null ? args.ChosenSuggestion.ToString() : args.QueryText;
            ViewModel.QueryCommand.Execute(uid);
        }

        private void CardAction_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string? uid = (string?)((CardAction)sender).CommandParameter;
            ViewModel.QueryCommand.Execute(uid);
        }
    }
}
