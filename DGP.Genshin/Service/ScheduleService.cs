using DGP.Genshin.Message;
using DGP.Genshin.Service.Abstraction;
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

        public ScheduleService(IMessenger messenger)
        {
            this.messenger = messenger;
        }

        public async void Initialize()
        {
            messenger.RegisterAll(this);
            try
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        double minutes = Setting2.ResinRefreshMinutes.Get();
                        await Task.Delay(TimeSpan.FromMinutes(minutes), cancellationTokenSource.Token);
                        this.Log("Tick scheduled");
                        messenger.Send(new TickScheduledMessage());
                    }
                }, cancellationTokenSource.Token)/*.ConfigureAwait(false)*/;
            }
            catch (TaskCanceledException) { }
        }
        public void UnInitialize()
        {
            cancellationTokenSource.Cancel();
            messenger.UnregisterAll(this);
        }
        public void Receive(AppExitingMessage message)
        {
            UnInitialize();
        }
    }
}
