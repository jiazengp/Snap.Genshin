using DGP.Genshin.Message;
using DGP.Genshin.Service.Abstraction;
using DGP.Genshin.Service.Abstraction.Setting;
using Microsoft.Toolkit.Mvvm.Messaging;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DGP.Genshin.Service
{
    [Service(typeof(IScheduleService), InjectAs.Singleton)]
    internal class ScheduleService : IScheduleService, IRecipient<AppExitingMessage>
    {
        private readonly CancellationTokenSource cancellationTokenSource = new();
        private readonly IMessenger messenger;

        private DateTime lastScheduledTime = DateTime.UtcNow + TimeSpan.FromHours(8);

        public ScheduleService(IMessenger messenger)
        {
            this.messenger = messenger;
        }

        public async Task InitializeAsync()
        {
            this.messenger.RegisterAll(this);
            try
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        double minutes = Setting2.ResinRefreshMinutes;
                        await Task.Delay(TimeSpan.FromMinutes(minutes), this.cancellationTokenSource.Token);
                        //await Task.Delay(10000, cancellationTokenSource.Token);
                        this.Log("Tick scheduled");
                        this.messenger.Send(new TickScheduledMessage());
                        DateTime current = DateTime.UtcNow + TimeSpan.FromHours(8);
                        if (current.Date > this.lastScheduledTime.Date)
                        {
                            this.Log("Date changed");
                            this.messenger.Send(new DayChangedMessage());
                        }
                        this.lastScheduledTime = current;
                    }
                }, this.cancellationTokenSource.Token);
            }
            catch (TaskCanceledException) { }
        }
        public void UnInitialize()
        {
            this.cancellationTokenSource.Cancel();
            this.messenger.UnregisterAll(this);
        }
        public void Receive(AppExitingMessage message)
        {
            this.UnInitialize();
        }
    }
}
