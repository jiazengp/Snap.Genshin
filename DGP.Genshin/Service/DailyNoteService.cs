using DGP.Genshin.DataModel.Cookie;
using DGP.Genshin.DataModel.DailyNote;
using DGP.Genshin.Helper.Notification;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.Service
{
    [Service(typeof(IDailyNoteService), InjectAs.Singleton)]
    internal class DailyNoteService : IDailyNoteService, IRecipient<CookieAddedMessage>, IRecipient<TickScheduledMessage>
    {
        private readonly ICookieService cookieService;
        private readonly IScheduleService scheduleService;
        private ConcurrentDictionary<CookieUserGameRole, DailyNote?> DailyNotes { get; set; } = new();

        public DailyNoteService(ICookieService cookieService, IScheduleService scheduleService, IMessenger messenger)
        {
            this.cookieService = cookieService;
            this.scheduleService = scheduleService;
        }

        public async void Initialize()
        {
            App.Messenger.RegisterAll(this);
            scheduleService.Initialize();
            await UpdateDailyNotesAsync();
        }

        public void UnInitialize()
        {
            scheduleService.UnInitialize();
            App.Messenger.UnregisterAll(this);
        }

        public DailyNote? GetDailyNote(CookieUserGameRole cookieUserGameRole)
        {
            if (DailyNotes.ContainsKey(cookieUserGameRole))
            {
                return DailyNotes[cookieUserGameRole];
            }
            else
            {
                return null;
            }
        }

        public async void Receive(TickScheduledMessage message)
        {
            this.Log("Tick received");
            await UpdateDailyNotesAsync();
        }

        private readonly TaskPreventer taskPreventer = new();

        private async Task UpdateDailyNotesAsync()
        {
            if (taskPreventer.ShouldExecute)
            {
                ConcurrentDictionary<CookieUserGameRole, DailyNote?> dailyNotes = new();
                foreach (string cookie in cookieService.Cookies)
                {
                    List<UserGameRole> userGameRoles = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();
                    DailyNoteProvider dailyNoteProvider = new(cookie);
                    foreach (UserGameRole userGameRole in userGameRoles)
                    {
                        DailyNote? dailyNote = await dailyNoteProvider.GetDailyNoteAsync(userGameRole);
                        dailyNotes[new(cookie, userGameRole)] = dailyNote;
                        App.Current.Dispatcher.Invoke(() => TriggerDailyNoteNotifications(userGameRole, dailyNote));
                    }
                }
                DailyNotes = dailyNotes;
                App.Messenger.Send(new DailyNotesRefreshedMessage());
                taskPreventer.Release();
            }
        }

        private void TriggerDailyNoteNotifications(UserGameRole userGameRole, DailyNote? dailyNote)
        {
            if (dailyNote is not null)
            {
                DailyNoteNotifyConfiguration? notify = Setting2.DailyNoteNotifyConfiguration.Get() ?? new();
                Setting2.DailyNoteNotifyConfiguration.Set(notify);
                ToastContentBuilder builder = new ToastContentBuilder().AddAttributionText(userGameRole.ToString());

                bool hasEverAddText = false;
                if ((notify.NotifyOnResinReach155 && dailyNote.CurrentResin >= 155)
                    || (notify.NotifyOnResinReach120 && dailyNote.CurrentResin >= 120)
                    || (notify.NotifyOnResinReach80 && dailyNote.CurrentResin >= 80)
                    || (notify.NotifyOnResinReach40 && dailyNote.CurrentResin >= 40)
                    || (notify.NotifyOnResinReach20 && dailyNote.CurrentResin >= 20))
                {
                    hasEverAddText = true;
                    builder.AddText($"当前原粹树脂：{dailyNote.CurrentResin}");
                }
                if (notify.NotifyOnHomeCoinReach80Percent && (dailyNote.CurrentHomeCoin >= (dailyNote.MaxHomeCoin * 0.8)))
                {
                    hasEverAddText = true;
                    builder.AddText($"当前洞天宝钱：{dailyNote.CurrentHomeCoin}");
                }
                if (notify.NotifyOnDailyTasksIncomplete && (!dailyNote.IsExtraTaskRewardReceived))
                {
                    hasEverAddText = true;
                    builder.AddText(dailyNote.ExtraTaskRewardDescription);
                }
                if (notify.NotifyOnExpeditionsComplete && (dailyNote.Expeditions?.All(exp => exp.Status == "Finished") == true))
                {
                    hasEverAddText = true;
                    builder.AddText("探索派遣已完成");
                }

                builder
                    .AddButton(new ToastButton()
                        .SetContent("启动游戏")
                        .AddArgument("launch", "game")
                        .SetBackgroundActivation())
                    .AddButton(new ToastButtonDismiss("我知道了"));
                if (hasEverAddText)
                {
                    SecureToastNotificationContext.TryCatch(() => builder.Show());
                }
            }
        }

        public void Receive(CookieAddedMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
