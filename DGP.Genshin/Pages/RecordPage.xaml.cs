using DGP.Genshin.ViewModels;
using ModernWpf.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// RecordPage.xaml 的交互逻辑
    /// </summary>
    public partial class RecordPage : Page
    {
        public RecordPage()
        {
            DataContext = App.GetViewModel<RecordViewModel>();
            InitializeComponent();
        }

        private RecordViewModel ViewModel => (RecordViewModel)DataContext;

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string? uid = args.ChosenSuggestion != null ? args.ChosenSuggestion.ToString() : args.QueryText;
            await ViewModel.QueryCommand.ExecuteAsync(uid);
        }
    }
}
