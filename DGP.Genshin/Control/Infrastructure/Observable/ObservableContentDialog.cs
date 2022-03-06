using ModernWpf.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DGP.Genshin.Control.Infrastructure.Observable
{
    public class ObservableContentDialog : ContentDialog, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null!)
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
    }
}
