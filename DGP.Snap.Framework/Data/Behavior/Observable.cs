using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DGP.Snap.Framework.Data.Behavior
{
    /// <summary>
    /// 实现 <see cref="INotifyPropertyChanged"/> 接口
    /// </summary>
    public class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
