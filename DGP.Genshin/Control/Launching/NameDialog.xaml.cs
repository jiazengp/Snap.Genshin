using DGP.Genshin.DataModel.Launching;
using ModernWpf.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DGP.Genshin.Control.Launching
{
    /// <summary>
    /// NameDialog.xaml 的交互逻辑
    /// </summary>
    public partial class NameDialog : ContentDialog, INotifyPropertyChanged
    {
        private string? input;
        private GenshinAccount? targetAccount;

        public string? Input { get => input; set => Set(ref input, value); }
        public GenshinAccount? TargetAccount { get => targetAccount; set => Set(ref targetAccount, value); }

        public NameDialog()
        {
            DataContext = this;
            InitializeComponent();
        }

        public async Task<string?> GetInputAsync()
        {
            await ShowAsync();
            return Input;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
