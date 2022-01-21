using DGP.Genshin.Controls;
using DGP.Genshin.Controls.GenshinElements;
using DGP.Genshin.Helpers;
using DGP.Genshin.MiHoYoAPI.Announcement;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(InjectAs.Transient)]
    public class HomeViewModel : ObservableObject
    {
        private IAsyncRelayCommand initializeCommand;
        private AnnouncementWrapper? announcement;

        public AnnouncementWrapper? Announcement
        {
            get => announcement;
            set => SetProperty(ref announcement, value);
        }
        public IAsyncRelayCommand OpenUICommand
        {
            get => initializeCommand;
            [MemberNotNull(nameof(initializeCommand))]
            set => SetProperty(ref initializeCommand, value);
        }

        public ICommand OpenAnnouncementUICommand
        {
            get;
        }
        public HomeViewModel()
        {
            OpenUICommand = new AsyncRelayCommand(InitializeAsync);
            OpenAnnouncementUICommand = new RelayCommand<string>(NavigateToAnnouncement);
        }

        /// <summary>
        /// 加载公告与活动
        /// </summary>
        private async Task InitializeAsync()
        {
            AnnouncementProvider provider = new();
            AnnouncementWrapper? wrapper = await provider.GetAnnouncementWrapperAsync();
            List<AnnouncementContent> contents = await provider.GetAnnouncementContentsAsync();

            Dictionary<int, string?> contentMap = contents.ToDictionary(id => id.AnnId, iContent => iContent.Content);
            wrapper?.List?.Reverse();
            wrapper?.List?.ForEach(listWrapper =>
            {
                listWrapper.List?.ForEach(item =>
{
    item.Content = contentMap[item.AnnId];
    item.OpenAnnouncementUICommand = OpenAnnouncementUICommand;
});
            });
            wrapper?.List?[0].List?.ForEach(item =>
            {
                Match match = Regex.Match(item.Content ?? "", @"(\d+\/\d+\/\d+\s\d+:\d+:\d+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (match.Success)
                {
                    DateTime time = DateTime.Parse(match.Value);
                    if (time > item.StartTime && time < item.EndTime)
                    {
                        item.StartTime = time;
                    }
                }
            });

            if (wrapper?.List?[0].List is not null)
            {
                wrapper.List[0].List = wrapper?.List?[0].List?.OrderBy(i => i.StartTime).ThenBy(i => i.EndTime).ToList();
            }

            Announcement = wrapper;
        }
        private void NavigateToAnnouncement(string? content)
        {
            if (WebView2Helper.IsSupported)
            {
                using (AnnouncementWindow? window = new AnnouncementWindow(content))
                {
                    window.ShowDialog();
                }
            }
            else
            {
                new WebView2RuntimeWindow().ShowDialog();
            }
        }
    }
}
