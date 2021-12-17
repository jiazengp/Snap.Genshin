using DGP.Genshin.DataModels.MiHoYo2;
using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.ViewModels;
using ModernWpf.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        #region AutoSuggest
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason is AutoSuggestionBoxTextChangeReason.UserInput or AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
            {
                IEnumerable<string>? result =
                    ViewModel.RecordService.QueryHistory?.Where(i => string.IsNullOrEmpty(sender.Text) || i.Contains(sender.Text));
                sender.ItemsSource = result?.Count() == 0 ? new List<string> { "暂无记录" } : result;
            }
        }
        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = (string)args.SelectedItem;
        }

        private bool opLocker = false;

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (opLocker)
            {
                return;
            }
            opLocker = true;
            string? uid = args.ChosenSuggestion != null ? args.ChosenSuggestion.ToString() : args.QueryText;
            Record record = await ViewModel.RecordService.GetRecordAsync(uid);

            if (record.Success)
            {
                IRecordService service = ViewModel.RecordService;
                //so that current record is always has data
                ViewModel.CurrentRecord = record;
                service.AddQueryHistory(uid);
            }
            else
            {
                if (record.Message?.Length == 0)
                {
                    await new ContentDialog()
                    {
                        Title = "查询失败",
                        Content = "米游社用户信息不完整，请在米游社完善个人信息。",
                        PrimaryButtonText = "确认",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                    Process.Start("https://bbs.mihoyo.com/ys/");
                }
                else
                {
                    await new ContentDialog()
                    {
                        Title = "查询失败",
                        Content = $"UID:{uid}\n{record.Message}\n更多信息请联系开发人员确认。",
                        PrimaryButtonText = "确认",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
            }
            opLocker = false;
        }
        #endregion
    }
}
