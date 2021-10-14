using DGP.Genshin.Models.MiHoYo.Record;
using DGP.Genshin.Services;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// CelestiaDatabasePage.xaml 的交互逻辑
    /// </summary>
    public partial class CelestiaDatabasePage : ModernWpf.Controls.Page
    {
        public RecordService RecordServiceView { get; set; } = RecordService.Instance;
        public CelestiaDatabasePage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = CelestiaDatabaseService.Instance;

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

            await CelestiaDatabaseService.Instance.InitializeAsync();
        }

        #region AutoSuggest
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
                    //so that current record is always has data
                    service.CurrentRecord = record;
                    if (CelestiaDatabaseService.Instance.IsInitialized)
                    {
                        await CelestiaDatabaseService.Instance.RefershRecommandsAsync();
                    }
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
        private void RecordService_RecordProgressed(string info) =>
            this.Dispatcher.Invoke(() => this.RequestingProgressText.Text = info);
        #endregion

        private async void PostUidAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            string result = "当前无可用玩家信息";
            if (RecordService.Instance.CurrentRecord != null)
            {
                result = await CelestiaDatabaseService.Instance.PostUid(RecordService.Instance.CurrentRecord.UserId);
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

    public class BooleanToStyleConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool boolean)
            {
                flag = boolean;
            }
            else if (value is bool?)
            {
                bool? flag2 = (bool?)value;
                flag = flag2.HasValue && flag2.Value;
            }

            return (!flag) ? this.FalseStyle : this.TrueStyle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        public Style TrueStyle
        {
            get => (Style)GetValue(TrueStyleProperty);
            set => SetValue(TrueStyleProperty, value);
        }
        public static readonly DependencyProperty TrueStyleProperty =
            DependencyProperty.Register("TrueStyle", typeof(Style), typeof(BooleanToStyleConverter), new PropertyMetadata(null));

        public Style FalseStyle
        {
            get => (Style)GetValue(FalseStyleProperty);
            set => SetValue(FalseStyleProperty, value);
        }
        public static readonly DependencyProperty FalseStyleProperty =
            DependencyProperty.Register("FalseStyle", typeof(Style), typeof(BooleanToStyleConverter), new PropertyMetadata(null));
    }
}
