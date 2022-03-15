using DGP.Genshin.ViewModel;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using WPFUI.Controls;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class RecordPage : ModernWpf.Controls.Page
    {
        public RecordPage(RecordViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }

        private RecordViewModel ViewModel
        {
            get => (RecordViewModel)DataContext;
        }

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
