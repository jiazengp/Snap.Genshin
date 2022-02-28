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
        private readonly ILaunchService launchService;
        private readonly ISignInService signInService;

        public ICommand ShowMainWindowCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand RestartElevatedCommand { get; }
        public ICommand LaunchGameCommand { get; }
        public ICommand OpenLauncherCommand { get; }
        public ICommand SignInCommand { get; }

        public TaskbarIconViewModel(ILaunchService launchService,ISignInService signInService, IMessenger messenger) : base(messenger)
        {
            this.launchService = launchService;
            this.signInService = signInService;

            ShowMainWindowCommand = new RelayCommand(OpenMainWindow);
            ExitCommand = new RelayCommand(ExitApp);
            RestartElevatedCommand = new RelayCommand(RestartElevated);
            LaunchGameCommand = new AsyncRelayCommand(LaunchGameAsync);
            OpenLauncherCommand = new RelayCommand(OpenLauncher);
            SignInCommand = new AsyncRelayCommand(SignInAsync);
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
            await launchService.LaunchAsync(LaunchOption.FromCurrentSettings(), ex =>
            {
                SecureToastNotificationContext.TryCatch(() =>
                new ToastContentBuilder()
                    .AddText("启动游戏失败")
                    .AddText(ex.Message)
                    .Show());
            });
        }
        private void OpenLauncher()
        {
            launchService.OpenOfficialLauncher(ex =>
            {
                SecureToastNotificationContext.TryCatch(() =>
                new ToastContentBuilder()
                    .AddText("打开启动器失败")
                    .AddText(ex.Message)
                    .Show());
            });
        }
        private async Task SignInAsync()
        {
            await signInService.TrySignAllAccountsRolesInAsync();
        }
    }
}
