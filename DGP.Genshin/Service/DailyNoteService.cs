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
    internal class DailyNoteService : IDailyNoteService, IRecipient<TickScheduledMessage>
    {
        private readonly ICookieService cookieService;
        private readonly IScheduleService scheduleService;
        private readonly IMessenger messenger;

        private ConcurrentDictionary<CookieUserGameRole, DailyNote?> DailyNotes { get; set; } = new();

        private ConcurrentDictionary<UserGameRole, bool> ContinueNotifyResin { get; } = new();
        private ConcurrentDictionary<UserGameRole, bool> ContinueNotifyHomeCoin { get; } = new();
        private ConcurrentDictionary<UserGameRole, bool> ContinueNotifyDailyTask { get; } = new();
        private ConcurrentDictionary<UserGameRole, bool> ContinueNotifyExpedition { get; } = new();

        public DailyNoteService(ICookieService cookieService, IScheduleService scheduleService, IMessenger messenger)
        {
            this.messenger = messenger;
            this.cookieService = cookieService;
            this.scheduleService = scheduleService;
        }

        public async void Initialize()
        {
            messenger.RegisterAll(this);
            scheduleService.Initialize();
            await UpdateDailyNotesAsync();
        }

        public void UnInitialize()
        {
            scheduleService.UnInitialize();
            messenger.UnregisterAll(this);
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
                cookieService.CookiesLock.EnterReadLock();
                foreach (string cookie in cookieService.Cookies)
                {
                    List<UserGameRole> userGameRoles = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();
                    DailyNoteProvider dailyNoteProvider = new(cookie);
                    foreach (UserGameRole userGameRole in userGameRoles)
                    {
                        DailyNote? dailyNote = await dailyNoteProvider.GetDailyNoteAsync(userGameRole);
                        dailyNotes[new(cookie, userGameRole)] = dailyNote;
                        App.Current.Dispatcher.Invoke(() => 
                        {
                            TrySendDailyNoteNotification(userGameRole, dailyNote, ContinueNotifyResin, 
                                EvaluateResin, dailyNote => $"当前原粹树脂：{dailyNote.CurrentResin}");
                            TrySendDailyNoteNotification(userGameRole, dailyNote, ContinueNotifyHomeCoin,
                                EvaluateHomeCoin, dailyNote => $"当前洞天宝钱：{dailyNote.CurrentHomeCoin}");
                            TrySendDailyNoteNotification(userGameRole, dailyNote, ContinueNotifyDailyTask,
                                EvaluateDailyTask, dailyNote => dailyNote.ExtraTaskRewardDescription);
                            TrySendDailyNoteNotification(userGameRole, dailyNote, ContinueNotifyExpedition,
                                EvaluateExpedition, dailyNote => "探索派遣已完成");
                        });
                    }
                }
                cookieService.CookiesLock.ExitReadLock();
                DailyNotes = dailyNotes;
                messenger.Send(new DailyNotesRefreshedMessage());
                taskPreventer.Release();
            }
        }

        private void TrySendDailyNoteNotification(
            UserGameRole userGameRole, DailyNote? dailyNote,
            ConcurrentDictionary<UserGameRole, bool> notifyFlags,
            Func<DailyNoteNotifyConfiguration, DailyNote, bool> sendConditionEvaluator,
            Func<DailyNote, string> appendTextFunc)
        {
            if (dailyNote is not null)
            {
                //fetch out notify settings
                DailyNoteNotifyConfiguration? notify = Setting2.DailyNoteNotifyConfiguration.Get() ?? new();
                Setting2.DailyNoteNotifyConfiguration.Set(notify);
                //init toast builder
                ToastContentBuilder builder = new ToastContentBuilder().AddAttributionText(userGameRole.ToString());
                //prepare notify flag
                if (!notifyFlags.ContainsKey(userGameRole))
                {
                    notifyFlags[userGameRole] = true;
                }

                bool shouldSend = sendConditionEvaluator.Invoke(notify, dailyNote);

                if (shouldSend)
                {
                    builder.AddText(appendTextFunc.Invoke(dailyNote), hintMaxLines: 1);
                }
                else
                {
                    //condition not meet ,reset it.
                    ContinueNotifyResin[userGameRole] = true;
                }

                builder
                    .AddButton(new ToastButton()
                        .SetContent("开始游戏")
                        .AddArgument("launch", "game")
                        .SetBackgroundActivation())
                    .AddButton(new ToastButton()
                        .SetContent("打开启动器")
                        .AddArgument("launch", "launcher")
                        .SetBackgroundActivation())
                    .AddButton(new ToastButtonDismiss("我知道了"));

                if (notifyFlags[userGameRole] && shouldSend)
                {
                    SecureToastNotificationContext.TryCatch(() => builder.Show());
                    //once it send out , flag this role
                    notifyFlags[userGameRole] = false;
                }
            }
        }

        private bool EvaluateResin(DailyNoteNotifyConfiguration notify, DailyNote note)
        {
            return (notify.NotifyOnResinReach155 && note.CurrentResin >= 155) 
                || (notify.NotifyOnResinReach120 && note.CurrentResin >= 120)
                || (notify.NotifyOnResinReach80 && note.CurrentResin >= 80)
                || (notify.NotifyOnResinReach40 && note.CurrentResin >= 40)
                || (notify.NotifyOnResinReach20 && note.CurrentResin >= 20);
        }
        private bool EvaluateHomeCoin(DailyNoteNotifyConfiguration notify, DailyNote note)
        {
            return notify.NotifyOnHomeCoinReach80Percent && (note.CurrentHomeCoin >= (note.MaxHomeCoin * 0.8));
        }
        private bool EvaluateDailyTask(DailyNoteNotifyConfiguration notify, DailyNote note)
        {
            return notify.NotifyOnDailyTasksIncomplete && (!note.IsExtraTaskRewardReceived);
        }
        private bool EvaluateExpedition(DailyNoteNotifyConfiguration notify, DailyNote note)
        {
            return notify.NotifyOnExpeditionsComplete && (note.Expeditions?.All(exp => exp.Status == "Finished") == true);
        }
    }
}
