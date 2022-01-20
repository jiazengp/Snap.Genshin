using DGP.Genshin.MiHoYoAPI.Blackboard;
using DGP.Genshin.MiHoYoAPI.Post;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(InjectAs.Transient)]
    public class HomeViewModel : ObservableObject
    {
        private readonly ICookieService cookieService;

        private IEnumerable<Post>? posts;
        private IEnumerable<BlackboardEvent>? blackboardEvents;
        private IAsyncRelayCommand initializeCommand;

        public IEnumerable<Post>? Posts
        {
            get => posts;
            set => SetProperty(ref posts, value);
        }
        public IEnumerable<BlackboardEvent>? BlackboardEvents
        {
            get => blackboardEvents;
            set => SetProperty(ref blackboardEvents, value);
        }
        public IAsyncRelayCommand OpenUICommand
        {
            get => initializeCommand;
            [MemberNotNull(nameof(initializeCommand))]
            set => SetProperty(ref initializeCommand, value);
        }

        public HomeViewModel(ICookieService cookieService)
        {
            this.cookieService = cookieService;
            OpenUICommand = new AsyncRelayCommand(InitializeAsync);
        }

        /// <summary>
        /// 加载公告与活动
        /// </summary>
        /// <returns></returns>
        private async Task InitializeAsync()
        {
            List<Post> posts = await new PostProvider(cookieService.CurrentCookie).GetOfficialRecommendedPostsAsync();
            Posts = posts.OrderBy(p => p.OfficialType);
            List<BlackboardEvent> events = await new BlackboardProvider().GetBlackboardEventsAsync();
            BlackboardEvents = events.Where(b => b.Kind == "1");
        }
    }
}
