using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Controls.TitleBarButtons;
using DGP.Genshin.Messages;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Journey;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using ModernWpf.Controls.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.ViewModels
{
    /// <summary>
    /// 旅行札记服务
    /// </summary>
    [ViewModel(ViewModelType.Transient)]
    public class JourneyViewModel : ObservableObject, IRecipient<CookieChangedMessage>
    {
        private readonly ICookieService cookieService;

        private JourneyProvider journeyProvider;
        private UserGameRoleProvider userGameRoleProvider;

        public JourneyViewModel(ICookieService cookieService)
        {
            this.cookieService = cookieService;
            InitializeCommand = new AsyncRelayCommand<TitleBarButton>(InitializeAsync);
            journeyProvider = new JourneyProvider(this.cookieService.CurrentCookie);
            userGameRoleProvider = new UserGameRoleProvider(this.cookieService.CurrentCookie);
        }

        private async Task InitializeAsync(TitleBarButton? t)
        {
            if (t?.ShowAttachedFlyout<Grid>(this) == true)
            {
                await InitializeInternalAsync();
            }
        }
        private async Task InitializeInternalAsync()
        {
            UserGameRoleInfo = await userGameRoleProvider.GetUserGameRolesAsync();
            SelectedRole = UserGameRoleInfo?.List?.FirstOrDefault(i => i.IsChosen);
        }

        #region Observable
        private JourneyInfo? journeyInfo;
        private UserGameRoleInfo? userGameRoleInfo;
        private UserGameRole? selectedRole;
        private IAsyncRelayCommand<TitleBarButton> initializeCommand;

        public JourneyInfo? JourneyInfo { get => journeyInfo; set => SetProperty(ref journeyInfo, value); }
        public UserGameRoleInfo? UserGameRoleInfo { get => userGameRoleInfo; set => SetProperty(ref userGameRoleInfo, value); }
        public UserGameRole? SelectedRole
        {
            get => selectedRole; set
            {
                SetProperty(ref selectedRole, value);
                UpdateJourneyInfo(value);
            }
        }
        private async void UpdateJourneyInfo(UserGameRole? role)
        {
            JourneyInfo = await journeyProvider.GetMonthInfoAsync(role?.GameUid, role?.Region);
        }

        public IAsyncRelayCommand<TitleBarButton> InitializeCommand
        {
            get => initializeCommand;

            [MemberNotNull("initializeCommand")]
            set => SetProperty(ref initializeCommand, value);
        }
        #endregion

        public void Receive(CookieChangedMessage message)
        {
            journeyProvider = new JourneyProvider(message.Value);
            userGameRoleProvider = new UserGameRoleProvider(message.Value);
        }
    }
}
