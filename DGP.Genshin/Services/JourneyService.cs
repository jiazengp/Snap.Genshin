using DGP.Genshin.Common.Data.Behavior;
using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Journey;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// 旅行札记服务
    /// </summary>
    public class JourneyService : Observable
    {
        private JourneyProvider journeyProvider;
        private UserGameRoleProvider userGameRoleProvider;
        public JourneyService()
        {
            CookieManager.CookieChanged += OnCookieChanged;
            journeyProvider = new JourneyProvider(CookieManager.CurrentCookie);
            userGameRoleProvider = new UserGameRoleProvider(CookieManager.CurrentCookie);
            UserGameRoleChanged += UpdateJourneyInfo;
        }

        private void OnCookieChanged()
        {
            journeyProvider = new JourneyProvider(CookieManager.CurrentCookie);
            userGameRoleProvider = new UserGameRoleProvider(CookieManager.CurrentCookie);
        }

        #region Observable
        private JourneyInfo? journeyInfo;
        private UserGameRoleInfo? userGameRoleInfo;
        private UserGameRole? selectedRole;

        public JourneyInfo? JourneyInfo { get => journeyInfo; set => Set(ref journeyInfo, value); }
        public UserGameRoleInfo? UserGameRoleInfo { get => userGameRoleInfo; set => Set(ref userGameRoleInfo, value); }
        public UserGameRole? SelectedRole
        {
            get => selectedRole; set
            {
                Set(ref selectedRole, value);
                UserGameRoleChanged?.Invoke(value);
            }
        }
        #endregion

        public async Task InitializeAsync()
        {
            UserGameRoleInfo = await userGameRoleProvider.GetUserGameRolesAsync();
            SelectedRole = UserGameRoleInfo?.List?.First();
        }

        private event Action<UserGameRole?> UserGameRoleChanged;
        private async void UpdateJourneyInfo(UserGameRole? role)
        {
            JourneyInfo = await journeyProvider.GetMonthInfoAsync(role?.GameUid, role?.Region);
        }
    }
}
