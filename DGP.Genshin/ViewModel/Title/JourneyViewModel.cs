using DGP.Genshin.Control.Title;
using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Journey;
using DGP.Genshin.Service.Abstratcion;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using ModernWpf.Controls.Primitives;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using Snap.Extenion.Enumerable;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel.Title
{
    /// <summary>
    /// 旅行札记服务
    /// </summary>
    [ViewModel(InjectAs.Transient)]
    public class JourneyViewModel : ObservableRecipient2, IRecipient<CookieChangedMessage>
    {
        private readonly ICookieService cookieService;

        private JourneyProvider journeyProvider;
        private UserGameRoleProvider userGameRoleProvider;

        private JourneyInfo? journeyInfo;
        private List<UserGameRole> userGameRoles = new();
        private UserGameRole? selectedRole;

        public JourneyInfo? JourneyInfo { get => journeyInfo; set => SetProperty(ref journeyInfo, value); }
        public List<UserGameRole> UserGameRoles { get => userGameRoles; set => SetProperty(ref userGameRoles, value); }
        public UserGameRole? SelectedRole
        {
            get => selectedRole;
            set => SetPropertyAndCallbackOnCompletion(ref selectedRole, value,
                async role => { JourneyInfo = await journeyProvider.GetMonthInfoAsync(role?.GameUid, role?.Region); });
        }

        public ICommand OpenUICommand { get; }

        public JourneyViewModel(ICookieService cookieService, IMessenger messenger) : base(messenger)
        {
            this.cookieService = cookieService;

            journeyProvider = new JourneyProvider(this.cookieService.CurrentCookie);
            userGameRoleProvider = new UserGameRoleProvider(this.cookieService.CurrentCookie);

            OpenUICommand = new AsyncRelayCommand<TitleBarButton>(OpenUIAsync);
        }

        private async Task OpenUIAsync(TitleBarButton? t)
        {
            if (t?.ShowAttachedFlyout<Grid>(this) == true)
            {
                new Event(t.GetType(), true).TrackAs(Event.OpenTitle);
                await InitializeInternalAsync();
            }
        }
        private async Task InitializeInternalAsync()
        {
            UserGameRoles = await userGameRoleProvider.GetUserGameRolesAsync();
            SelectedRole = UserGameRoles.MatchedOrFirst(i => i.IsChosen);
        }

        public void Receive(CookieChangedMessage message)
        {
            journeyProvider = new JourneyProvider(message.Value);
            userGameRoleProvider = new UserGameRoleProvider(message.Value);
        }
    }
}
