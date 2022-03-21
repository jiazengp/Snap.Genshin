using DGP.Genshin.Core.Notification;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Service.Abstraction;
using DGP.Genshin.Service.Abstraction.Launching;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
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
        public ICommand RestartAsElevatedCommand { get; }
        public ICommand LaunchGameCommand { get; }
        public ICommand OpenLauncherCommand { get; }
        public ICommand SignInCommand { get; }

        public TaskbarIconViewModel(ISignInService signInService, IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger) : base(messenger)
        {
            launchService = App.Current.SwitchableImplementationManager.CurrentLaunchService!.Factory.Value;
            this.signInService = signInService;

            ShowMainWindowCommand = new RelayCommand(OpenMainWindow);
            ExitCommand = new RelayCommand(ExitApp);
            RestartAsElevatedCommand = new RelayCommand(App.RestartAsElevated);
            LaunchGameCommand = asyncRelayCommandFactory.Create(LaunchGameAsync);
            OpenLauncherCommand = new RelayCommand(OpenLauncher);
            SignInCommand = asyncRelayCommandFactory.Create(SignInAsync);
        }

        private void OpenMainWindow()
        {
            App.BringWindowToFront<MainWindow>();
        }
        private void ExitApp()
        {
            App.Current.Shutdown();
        }
        private async Task LaunchGameAsync()
        {
            await launchService.LaunchAsync(LaunchOption.FromCurrentSettings(), ex =>
            {
                new ToastContentBuilder()
                    .AddText("启动游戏失败")
                    .AddText(ex.Message)
                    .SafeShow();
            });
        }
        private void OpenLauncher()
        {
            launchService.OpenOfficialLauncher(ex =>
            {
                new ToastContentBuilder()
                    .AddText("打开启动器失败")
                    .AddText(ex.Message)
                    .Show();
            });
        }
        private async Task SignInAsync()
        {
            await signInService.TrySignAllAccountsRolesInAsync();
        }
    }
}
