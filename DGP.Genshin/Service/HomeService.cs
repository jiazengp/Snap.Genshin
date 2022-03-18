using DGP.Genshin.MiHoYoAPI.Announcement;
using DGP.Genshin.Service.Abstraction;
using Newtonsoft.Json;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Data.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.Service
{
    [Service(typeof(IHomeService), InjectAs.Transient)]
    internal class HomeService : IHomeService
    {
        public async Task<AnnouncementWrapper> GetAnnouncementsAsync(ICommand openAnnouncementUICommand, CancellationToken cancellationToken = default)
        {
            AnnouncementProvider provider = new();
            AnnouncementWrapper? wrapper = await provider.GetAnnouncementWrapperAsync(cancellationToken);
            List<AnnouncementContent> contents = new();
            try
            {
                contents = await provider.GetAnnouncementContentsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                this.Log(ex.Message);
            }
            Dictionary<int, string?> contentMap = contents.ToDictionary(id => id.AnnId, iContent => iContent.Content);
            if (wrapper is not null)
            {
                if (wrapper.List is List<AnnouncementListWrapper> announcementListWrappers)
                {
                    //将活动公告置于上方
                    announcementListWrappers.Reverse();
                    //匹配特殊的时间格式
                    Regex timeTagRegrex = new("&lt;t.*?&gt;(.*?)&lt;/t&gt;", RegexOptions.Multiline);
                    Regex timeTagInnerRegex = new("(?<=&lt;t.*?&gt;)(.*?)(?=&lt;/t&gt;)");

                    announcementListWrappers.ForEach(listWrapper =>
                    {
                        listWrapper.List?.ForEach(item =>
                        {
                            string? rawContent = contentMap[item.AnnId];
                            if (rawContent != null)
                            {
                                rawContent = timeTagRegrex.Replace(rawContent, x => timeTagInnerRegex.Match(x.Value).Value);
                            }
                            item.Content = rawContent;
                            item.OpenAnnouncementUICommand = openAnnouncementUICommand;
                        });
                    });

                    if (announcementListWrappers[0].List is List<Announcement> activities)
                    {
                        //Match d+/d+/d+ d+:d+:d+ time format
                        Regex dateTimeRegex = new(@"(\d+\/\d+\/\d+\s\d+:\d+:\d+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        activities.ForEach(item =>
                        {
                            Match matched = dateTimeRegex.Match(item.Content ?? "");
                            if (matched.Success && DateTime.TryParse(matched.Value, out DateTime time))
                            {
                                if (time > item.StartTime && time < item.EndTime)
                                {
                                    item.StartTime = time;
                                }
                            }
                        });

                        wrapper.List[0].List = activities
                            .OrderBy(i => i.StartTime)
                            .ThenBy(i => i.EndTime)
                            .ToList();
                    }
                    return wrapper;
                }
            }
            return new();
        }

        private record ManifestoWrapper([property: JsonProperty("manifesto")] string Manifesto);

        public async Task<string> GetManifestoAsync(CancellationToken cancellationToken = default)
        {
            ManifestoWrapper? manifesto = await Json
                .FromWebsiteAsync<ManifestoWrapper>("https://api.snapgenshin.com/manifesto")
                .ConfigureAwait(false);
            return manifesto?.Manifesto ?? "暂无 Snap Genshin 官方公告";
        }
    }
}
