using DGP.Genshin.Models.MiHoYo.Record;
using DGP.Genshin.Services;
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
            this.DataContext = RecordService.Instance;
            InitializeComponent();
            if (RecordService.Instance.QueryHistory.Count > 0)
            {
                RecordService s = RecordService.Instance;
                if (s.CurrentRecord != null && s.CurrentRecord?.UserId != null)
                {
                    this.QueryAutoSuggestBox.Text = s.CurrentRecord?.UserId;
                }
                else if (s.QueryHistory.Count > 0)
                {
                    this.QueryAutoSuggestBox.Text = s.QueryHistory.First();
                }
            }
        }
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason is AutoSuggestionBoxTextChangeReason.UserInput or AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
            {
                IEnumerable<string> result =
                    RecordService.Instance.QueryHistory.Where(i => System.String.IsNullOrEmpty(sender.Text) || i.Contains(sender.Text));
                sender.ItemsSource = result.Count() == 0 ? new List<string> { "暂无记录" } : result;
            }
        }
        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) =>
            sender.Text = (string)args.SelectedItem;

        private bool isQuerying = false;
        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!this.isQuerying)
            {
                this.isQuerying = true;
                this.RequestingProgressRing.IsActive = true;
                RecordService.RecordProgressed += RecordService_RecordProgressed;
                string uid = args.ChosenSuggestion != null ? args.ChosenSuggestion.ToString() : args.QueryText;
                Record record = await RecordService.Instance.GetRecordAsync(uid);
                RecordService.RecordProgressed -= RecordService_RecordProgressed;
                this.RequestingProgressRing.IsActive = false;

                if (record.Success)
                {
                    RecordService service = RecordService.Instance;
                    service.CurrentRecord = record;
                    service.SelectedAvatar = record.DetailedAvatars.First();
                    service.AddQueryHistory(uid);
                }
                else
                {
                    if (record.Message.Length == 0)
                    {
                        await new ContentDialog()
                        {
                            Title = "查询失败",
                            Content = "你的米游社用户信息可能不完整，请在米游社完善个人信息。",
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
                            Content = $"UID:{uid}\n{record.Message}",
                            PrimaryButtonText = "确认",
                            DefaultButton = ContentDialogButton.Primary
                        }.ShowAsync();
                    }
                }
                this.isQuerying = false;
            }
        }
        private void RecordService_RecordProgressed(string info) => this.RequestingProgressText.Text = info;
    }
}
