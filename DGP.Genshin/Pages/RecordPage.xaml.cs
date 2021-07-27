using DGP.Genshin.Models.MiHoYo.Record;
using DGP.Genshin.Services;
using DGP.Snap.Framework.Extensions.System;
using ModernWpf.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// RecordPage.xaml 的交互逻辑
    /// </summary>
    public partial class RecordPage : System.Windows.Controls.Page
    {
        public RecordPage()
        {
            if (!RecordAPI.Instance.GetLoginStatus())
            {
                RecordService.Instance.Login();
            }
            this.DataContext = RecordService.Instance;
            this.InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e) => RecordService.Instance.Login();

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                IEnumerable<string> result = RecordService.Instance.QueryHistory.Where(i => i.Contains(sender.Text));
                sender.ItemsSource = result.Count() == 0 ? new List<string> { "暂无历史搜索" } : result;
            }
        }
        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) =>
            // Set sender.Text. You can use args.SelectedItem to build your text string.
            sender.Text = (string)args.SelectedItem;
        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string uid = args.QueryText; ;
            string server = ((ComboBoxItem)this.ServerComboBox.SelectedItem).Tag.ToString();

            this.Log($"ticket:{RecordService.Instance.LoginTicket}");

            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
            }
            else
            {
                // Use args.QueryText to determine what to do.
            }
            Record record = RecordAPI.Instance.GetRecord(uid, server);
            if (!record.Success)
            {
                if (record.Message.Length == 0)
                {
                    new ContentDialog()
                    {
                        Title = "查询失败",
                        Content = "米游社用户信息可能不完整!\n请在米游社登录账号并完善个人信息\n完善后方可查询任意玩家信息",
                        PrimaryButtonText = "确认",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                    Process.Start("https://bbs.mihoyo.com/ys/");
                }
                else
                {
                    new ContentDialog()
                    {
                        Title = "查询失败",
                        Content = record.Message,
                        PrimaryButtonText = "确认",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
                return;
            }
            RecordService.Instance.CurrentRecord = record;
            RecordService.Instance.AddQueryHistory(uid);
        }
    }
}
