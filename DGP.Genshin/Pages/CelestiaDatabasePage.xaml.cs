using DGP.Genshin.DataModel.MiHoYo2;
using DGP.Genshin.Services.GameRecord;
using DGP.Genshin.ViewModels;
using DGP.Genshin.YoungMoeAPI;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// CelestiaDatabasePage.xaml 的交互逻辑
    /// </summary>
    public partial class CelestiaDatabasePage : Page
    {
        public CelestiaDatabasePage()
        {
            DataContext = App.GetViewModel<CelestiaDatabaseViewModel>();
            InitializeComponent();
        }

        #region AutoSuggest
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason is AutoSuggestionBoxTextChangeReason.UserInput or AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
            {
                IEnumerable<string>? result =
                    App.GetService<RecordService>().QueryHistory?.Where(i => string.IsNullOrEmpty(sender.Text) || i.Contains(sender.Text));
                sender.ItemsSource = result?.Count() == 0 ? new List<string> { "暂无记录" } : result;
            }
        }
        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = (string)args.SelectedItem;
        }

        private bool isQuerying = false;
        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!isQuerying)
            {
                isQuerying = true;
                RequestingProgressRing.IsActive = true;
                App.GetService<RecordService>().RecordProgressed += RecordService_RecordProgressed;
                string? uid = args.ChosenSuggestion != null ? args.ChosenSuggestion.ToString() : args.QueryText;
                if (uid is not null)
                {
                    Record record = await App.GetService<RecordService>().GetRecordAsync(uid);
                    App.GetService<RecordService>().RecordProgressed -= RecordService_RecordProgressed;
                    RequestingProgressRing.IsActive = false;

                    if (record.Success)
                    {
                        //so that current record is always has data
                        App.GetService<RecordService>().CurrentRecord = record;
                        if (App.GetViewModel<CelestiaDatabaseViewModel>().IsInitialized)
                        {
                            await App.GetViewModel<CelestiaDatabaseViewModel>().RefershRecommandsAsync();
                        }
                        App.GetService<RecordService>().AddQueryHistory(uid);
                    }
                    else
                    {
                        if (record.Message?.Length == 0)
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
                }
                isQuerying = false;
            }
        }
        private void RecordService_RecordProgressed(string? info)
        {
            Dispatcher.Invoke(() => RequestingProgressText.Text = info);
        }
        #endregion

        private async void PostUidAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            string? result = "当前无可用玩家信息";
            if (App.GetService<RecordService>().CurrentRecord?.UserId is not null)
            {
                result = await new CelestiaDatabaseService().PostUidAsync(App.GetService<RecordService>().CurrentRecord.UserId);
            }
            await new ContentDialog()
            {
                Title = "提交数据",
                Content = result,
                PrimaryButtonText = "确认",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
        }
    }
}