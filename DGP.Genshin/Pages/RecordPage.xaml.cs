using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Services.CelestiaDatabase;
using DGP.Genshin.Services.GameRecord;
using ModernWpf.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Navigation;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// RecordPage.xaml 的交互逻辑
    /// </summary>
    public partial class RecordPage : Page, INotifyPropertyChanged
    {
        public RecordPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DataContext = RecordService.Instance;
            if (RecordService.Instance.QueryHistory?.Count > 0)
            {
                RecordService s = RecordService.Instance;
            }
            this.Log("initialized");
        }

        #region AutoSuggest
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason is AutoSuggestionBoxTextChangeReason.UserInput or AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
            {
                IEnumerable<string>? result =
                    RecordService.Instance.QueryHistory?.Where(i => string.IsNullOrEmpty(sender.Text) || i.Contains(sender.Text));
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
            Record record = await RecordService.Instance.GetRecordAsync(uid);

            if (record.Success)
            {
                RecordService service = RecordService.Instance;
                //so that current record is always has data
                service.CurrentRecord = record;
                if (CelestiaDatabaseService.Instance.IsInitialized)
                {
                    await CelestiaDatabaseService.Instance.RefershRecommandsAsync();
                }
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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            RecordService.Instance.UnInitialize();
        }
    }
}
