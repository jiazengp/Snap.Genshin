using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace DGP.Genshin.DataModels.Behavior
{
    public class Checkable<T> : ObservableObject
    {
        private readonly Action<bool, T>? callback;
        private bool isChecked;
        private T _value;

        public bool IsChecked
        {
            get => isChecked;
            set
            {
                SetProperty(ref isChecked, value);
                callback?.Invoke(isChecked, _value);
            }
        }
        public T Value { get => _value; set => SetProperty(ref _value, value); }

        public Checkable(T value)
        {
            _value = value;
        }

        public Checkable(T value, Action<bool, T> checkChangedCallback)
        {
            callback = checkChangedCallback;
            _value = value;
        }
    }
}
