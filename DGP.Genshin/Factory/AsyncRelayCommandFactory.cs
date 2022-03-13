﻿using DGP.Genshin.Factory.Abstraction;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DGP.Genshin.Factory
{
    [Factory(typeof(IAsyncRelayCommandFactory), InjectAs.Transient)]
    internal class AsyncRelayCommandFactory : IAsyncRelayCommandFactory
    {
        public AsyncRelayCommand<T> Create<T>(Func<T?, Task> execute)
        {
            return Register(new AsyncRelayCommand<T>(execute));
        }

        public AsyncRelayCommand<T> Create<T>(Func<T?, CancellationToken, Task> cancelableExecute)
        {
            return Register(new AsyncRelayCommand<T>(cancelableExecute));
        }

        public AsyncRelayCommand<T> Create<T>(Func<T?, Task> execute, Predicate<T?> canExecute)
        {
            return Register(new AsyncRelayCommand<T>(execute, canExecute));
        }

        public AsyncRelayCommand<T> Create<T>(Func<T?, CancellationToken, Task> cancelableExecute, Predicate<T?> canExecute)
        {
            return Register(new AsyncRelayCommand<T>(cancelableExecute, canExecute));
        }

        public AsyncRelayCommand Create(Func<Task> execute)
        {
            return Register(new AsyncRelayCommand(execute));
        }

        public AsyncRelayCommand Create(Func<CancellationToken, Task> cancelableExecute)
        {
            return Register(new AsyncRelayCommand(cancelableExecute));
        }

        public AsyncRelayCommand Create(Func<Task> execute, Func<bool> canExecute)
        {
            return Register(new AsyncRelayCommand(execute, canExecute));
        }

        public AsyncRelayCommand Create(Func<CancellationToken, Task> cancelableExecute, Func<bool> canExecute)
        {
            return Register(new AsyncRelayCommand(cancelableExecute, canExecute));
        }

        private AsyncRelayCommand Register(AsyncRelayCommand command)
        {
            ReportException(command);
            return command;
        }

        private AsyncRelayCommand<T> Register<T>(AsyncRelayCommand<T> command)
        {
            ReportException(command);
            return command;
        }

        private void ReportException(IAsyncRelayCommand command)
        {
            command.PropertyChanged += (sender, args) =>
            {
                if (sender is null)
                {
                    return;
                }
                if (args.PropertyName == nameof(AsyncRelayCommand.ExecutionTask))
                {
                    if (sender is IAsyncRelayCommand asyncRelayCommand)
                    {
                        if (asyncRelayCommand.ExecutionTask?.Exception is AggregateException exception)
                        {
                            this.Log(exception);
                        }
                    }
                }
            };
        }
    }
}
