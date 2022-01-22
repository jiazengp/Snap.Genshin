using DGP.Genshin.DataModels.Cookies;
using DGP.Genshin.Messages;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Snap.Core.Mvvm;
using Snap.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DGP.Genshin.ViewModels
{
    /// <summary>
    /// 桌面上的实时便笺小组件
    /// 需要显式创建的视图
    /// </summary>
    public class DailyNoteResinViewModel : ObservableRecipient2, IRecipient<TickScheduledMessage>
    {
        private readonly TaskPreventer refreshDailyNoteTaskPreventer = new();

        public CookieUserGameRole CookieUserGameRole { get; }

        private DailyNote? dailyNote;

        public DailyNote? DailyNote { get => dailyNote; set => SetProperty(ref dailyNote, value); }
        public ICommand OpenUICommand { get; }

        public DailyNoteResinViewModel(CookieUserGameRole cookieUserGameRole, IMessenger messenger) : base(messenger)
        {
            CookieUserGameRole = cookieUserGameRole;
            OpenUICommand = new AsyncRelayCommand<Window>(OpenUIAsync);
        }

        private async Task OpenUIAsync(Window? window)
        {
            //window?.SetInDesktop();
            await UpdateInfoAsync();
        }

        /// <summary>
        /// 刷新实时便笺
        /// </summary>
        /// <returns></returns>
        private async Task UpdateInfoAsync()
        {
            if (refreshDailyNoteTaskPreventer.ShouldExecute)
            {
                UserGameRole role = CookieUserGameRole.UserGameRole;
                if (role.Region is not null && role.GameUid is not null)
                {
                    DailyNote = await new DailyNoteProvider(CookieUserGameRole.Cookie).GetDailyNoteAsync(role.Region, role.GameUid);
                }
                refreshDailyNoteTaskPreventer.Release();
            }
        }

        public async void Receive(TickScheduledMessage message)
        {
            await UpdateInfoAsync();
        }
    }
}
