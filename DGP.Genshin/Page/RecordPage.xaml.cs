using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.ViewModel;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using WPFUI.Controls;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class RecordPage : AsyncPage
    {
        public RecordPage(RecordViewModel vm) : base(vm)
        {
            this.InitializeComponent();
        }

        private RecordViewModel ViewModel
        {
            get => (RecordViewModel)this.DataContext;
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string? uid = args.ChosenSuggestion != null ? args.ChosenSuggestion.ToString() : args.QueryText;
            this.ViewModel.QueryCommand.Execute(uid);
        }

        private void CardAction_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string? uid = (string?)((CardAction)sender).CommandParameter;
            this.ViewModel.QueryCommand.Execute(uid);
        }
    }
}
