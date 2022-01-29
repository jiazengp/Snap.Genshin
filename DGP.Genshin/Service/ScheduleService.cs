using DGP.Genshin.Message;
using DGP.Genshin.Service.Abstratcion;
using Microsoft.Toolkit.Mvvm.Messaging;
using Snap.Core.DependencyInjection;
using System;
using System.Timers;

namespace DGP.Genshin.Service
{
    [Service(typeof(IScheduleService), InjectAs.Singleton)]
    internal class ScheduleService : IScheduleService, IDisposable, IRecipient<SettingChangedMessage>, IRecipient<AppExitingMessage>
    {
        private readonly Timer timer;
        private bool disposed;

        public ScheduleService(ISettingService settingService)
        {
            timer = new(); //new(DispatcherPriority.Background, App.Current.Dispatcher);
            double minutes = settingService.GetOrDefault(Setting.ResinRefreshMinutes, 8d);
            timer.Interval = TimeSpan.FromMinutes(minutes).TotalMilliseconds;
            timer.Elapsed += (s, e) => App.Messenger.Send(new TickScheduledMessage());
        }

        public void Initialize()
        {
            App.Messenger.RegisterAll(this);
            timer.Start();
        }
        public void UnInitialize()
        {
            timer.Stop();
            timer.Dispose();
            App.Messenger.UnregisterAll(this);
        }

        public void Receive(SettingChangedMessage message)
        {
            if (message.Value.Key == Setting.ResinRefreshMinutes)
            {
                //unbox to double
                double minutes = (double)message.Value.Value!;
                timer.Interval = TimeSpan.FromMinutes(minutes).TotalMilliseconds;
            }
        }
        public void Receive(AppExitingMessage message)
        {
            UnInitialize();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    timer.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposed = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~ScheduleService()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
