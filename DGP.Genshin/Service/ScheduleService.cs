using DGP.Genshin.Message;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Messaging;
using Snap.Core.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DGP.Genshin.Service
{
    [Service(typeof(IScheduleService), InjectAs.Singleton)]
    internal class ScheduleService : IScheduleService, IRecipient<AppExitingMessage>
    {
        private readonly ISettingService settingService;
        private readonly CancellationTokenSource cancellationTokenSource = new();

        public ScheduleService(ISettingService settingService)
        {
            this.settingService = settingService;
        }

        public async void Initialize()
        {
            App.Messenger.RegisterAll(this);
            try
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        double minutes = settingService.GetOrDefault(Setting.ResinRefreshMinutes, 8d);
                        await Task.Delay(TimeSpan.FromMinutes(minutes), cancellationTokenSource.Token);
                        App.Messenger.Send(new TickScheduledMessage());
                    }
                }, cancellationTokenSource.Token)/*.ConfigureAwait(false)*/;
            }
            catch (TaskCanceledException) { }
        }
        public void UnInitialize()
        {
            cancellationTokenSource.Cancel();
            App.Messenger.UnregisterAll(this);
        }
        public void Receive(AppExitingMessage message)
        {
            UnInitialize();
        }
    }
}
