using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.Journey;
using DGP.Genshin.MiHoYoAPI.User;
using DGP.Snap.Framework.Data.Behavior;
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
        private readonly JourneyProvider journeyProvider;
        private readonly UserGameRoleProvider userGameRoleProvider;
        public JourneyService()
        {
            journeyProvider = new JourneyProvider(CookieManager.Cookie);
            userGameRoleProvider = new UserGameRoleProvider(CookieManager.Cookie);
            UserGameRoleChanged += UpdateJourneyInfo;
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
            UserGameRoleInfo = await Task.Run(() =>
            userGameRoleProvider.GetUserGameRoles());
            SelectedRole = UserGameRoleInfo?.List?.First();
        }

        private event Action<UserGameRole?> UserGameRoleChanged;
        private async void UpdateJourneyInfo(UserGameRole? role)
        {
            JourneyInfo = await Task.Run(() => journeyProvider.GetMonthInfo(role?.GameUid, role?.Region));
        }
    }
}
