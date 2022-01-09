using DGP.Genshin.Controls.GenshinElements;
using DGP.Genshin.DataModels.Cookies;
using DGP.Genshin.ViewModels;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace DGP.Genshin.DataModels.DailyNotes
{
    /// <summary>
    /// 请确保<see cref="CookieUserGameRole"/> 不为 null
    /// </summary>
    public class ResinWidgetConfigration : ObservableObject
    {
        [JsonProperty] public bool IsPresent { get; set; }
        [JsonProperty] public CookieUserGameRole? CookieUserGameRole { get; set; }
        [JsonProperty] public double Top { get; set; }
        [JsonProperty] public double Left { get; set; }

        [JsonIgnore]
        public DailyNoteWindow? DailyNoteWindow { get; set; }

        private bool isChecked;
        /// <summary>
        /// 决定实时树脂窗体是否应显示
        /// </summary>
        [JsonIgnore]
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                IsPresent = value;
                SetProperty(ref isChecked, value);
                UpdateWidgetState();
            }
        }

        /// <summary>
        /// 使用内部信息更新调整窗口状态
        /// </summary>
        private void UpdateWidgetState()
        {
            if (IsChecked)
            {
                DailyNoteWindow = new DailyNoteWindow(new DailyNoteResinViewModel(CookieUserGameRole!, App.Messenger))
                {
                    Left = Left,
                    Top = Top
                };
                DailyNoteWindow.Show();
            }
            else
            {
                DailyNoteWindow?.Close();
                DailyNoteWindow = null;
            }
        }

        /// <summary>
        /// 将小组件UI初始化
        /// </summary>
        public void Initialize()
        {
            IsChecked = IsPresent;
        }

        /// <summary>
        /// 更新内部的属性信息，从窗口获取并同步当前状态
        /// </summary>
        internal void UpdatePropertyState()
        {
            if (DailyNoteWindow is not null)
            {
                Top = DailyNoteWindow.Top;
                Left = DailyNoteWindow.Left;
            }
        }
    }
}
