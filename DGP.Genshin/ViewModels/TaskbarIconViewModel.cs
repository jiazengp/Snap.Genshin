using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Controls.GenshinElements;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
    internal class TaskbarIconViewModel : ObservableObject
    {
        private ICommand showMainWindowCommand;
        private ICommand exitCommand;
        private ICommand dailyNoteWindowCommand;

        public ICommand ShowMainWindowCommand
        {
            get => showMainWindowCommand;
            [MemberNotNull(nameof(showMainWindowCommand))]
            set => SetProperty(ref showMainWindowCommand, value);
        }
        public ICommand DailyNoteWindowCommand
        {
            get => dailyNoteWindowCommand;
            [MemberNotNull(nameof(dailyNoteWindowCommand))]
            set => SetProperty(ref dailyNoteWindowCommand, value);
        }
        public ICommand ExitCommand
        {
            get => exitCommand;
            [MemberNotNull(nameof(exitCommand))]
            set => SetProperty(ref exitCommand, value);
        }

        public TaskbarIconViewModel()
        {
            ShowMainWindowCommand = new RelayCommand(OpenMainWindow);
            DailyNoteWindowCommand = new RelayCommand(OpenDailyNoteWindow);
            ExitCommand = new RelayCommand(ExitApp);
        }

        private void OpenMainWindow()
        {
            App.ShowOrCloseWindow<MainWindow>();
        }
        private void OpenDailyNoteWindow()
        {
            App.ShowOrCloseWindow<DailyNoteWindow>();
        }
        private void ExitApp()
        {
            App.Current.Shutdown();
        }
    }
}
