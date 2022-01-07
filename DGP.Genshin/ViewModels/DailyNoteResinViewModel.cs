using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Common.Threading;
using DGP.Genshin.DataModels.Cookies;
using DGP.Genshin.Helpers;
using DGP.Genshin.Messages;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DGP.Genshin.ViewModels
{
    /// <summary>
    /// 实时
    /// 需要显示创建的视图
    /// </summary>
    public class DailyNoteResinViewModel : ObservableRecipient, IRecipient<TickScheduledMessage>
    {
        public CookieUserGameRole CookieUserGameRole { get; }

        private DailyNote? dailyNote;
        private ICommand openUICommand;

        public DailyNote? DailyNote { get => dailyNote; set => SetProperty(ref dailyNote, value); }
        public ICommand OpenUICommand
        {
            get => openUICommand;
            [MemberNotNull(nameof(openUICommand))]
            set => openUICommand = value;
        }

        public DailyNoteResinViewModel(CookieUserGameRole cookieUserGameRole, IMessenger messenger) : base(messenger)
        {
            CookieUserGameRole = cookieUserGameRole;
            OpenUICommand = new AsyncRelayCommand<Window>(OpenUIAsync);

            IsActive = true;
        }

        ~DailyNoteResinViewModel()
        {
            IsActive = false;
        }

        /// <summary>
        /// 打开浮出控件
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private async Task OpenUIAsync(Window? window)
        {
            //window?.SetInDesktop();
            await UpdateInfoAsync();
        }

        private readonly TaskPreventer refreshDailyNoteTaskPreventer = new();

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
