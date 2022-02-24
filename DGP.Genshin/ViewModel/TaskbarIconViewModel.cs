using DGP.Genshin.Helper;
using DGP.Genshin.Helper.Notification;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class TaskbarIconViewModel : ObservableRecipient2
    {
        public ICommand ShowMainWindowCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand RestartElevatedCommand { get; }
        public ICommand LaunchGameCommand { get; set; }

        public TaskbarIconViewModel(IMessenger messenger) : base(messenger)
        {
            ShowMainWindowCommand = new RelayCommand(OpenMainWindow);
            ExitCommand = new RelayCommand(ExitApp);
            RestartElevatedCommand = new RelayCommand(RestartElevated);
            LaunchGameCommand = new AsyncRelayCommand(LaunchGameAsync);
        }

        private void OpenMainWindow()
        {
            App.Current.Dispatcher.Invoke(() => App.BringWindowToFront<MainWindow>());
        }
        private void ExitApp()
        {
            App.Current.Shutdown();
        }
        private void RestartElevated()
        {
            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    FileName = PathContext.Locate("DGP.Genshin.exe"),
                });
            }
            catch (Win32Exception)
            {
                return;
            }

            App.Current.Shutdown();
        }
        private async Task LaunchGameAsync()
        {
            LaunchOption? launchOption = new()
            {
                IsBorderless = Setting2.IsBorderless.Get(),
                IsFullScreen = Setting2.IsFullScreen.Get(),
                UnlockFPS = App.IsElevated && Setting2.UnlockFPS.Get(),
                TargetFPS = (int)Setting2.TargetFPS.Get(),
                ScreenWidth = (int)Setting2.ScreenWidth.Get(),
                ScreenHeight = (int)Setting2.ScreenHeight.Get()
            };
            await App.AutoWired<ILaunchService>().LaunchAsync(launchOption, ex =>
            {
                SecureToastNotificationContext.TryCatch(() =>
                new ToastContentBuilder()
                    .AddText("启动游戏失败")
                    .AddText(ex.Message)
                    .Show());
            });
        }
    }
}
