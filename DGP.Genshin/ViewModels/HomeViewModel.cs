using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.MiHoYoAPI.Blackboard;
using DGP.Genshin.MiHoYoAPI.Post;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
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

        public IAsyncRelayCommand InitializeCommand
        {
            get => initializeCommand;
            [MemberNotNull(nameof(initializeCommand))]
            set => SetProperty(ref initializeCommand, value);
        }

        public HomeViewModel(ICookieService cookieService)
        {
            this.cookieService = cookieService;
            InitializeCommand = new AsyncRelayCommand(InitializeAsync);
        }

        private async Task InitializeAsync()
        {
            Posts = (await new PostProvider(cookieService.CurrentCookie).GetOfficialRecommendedPostsAsync()).OrderBy(p => p.OfficialType);
            BlackboardEvents = (await new BlackboardProvider().GetBlackboardEventsAsync()).Where(b => b.Kind == "1");
        }
    }
}
