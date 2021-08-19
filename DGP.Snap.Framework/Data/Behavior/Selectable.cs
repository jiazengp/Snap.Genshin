using System;

namespace DGP.Snap.Framework.Data.Behavior
{
    public class Selectable<T> : Observable
    {
        private T _value;
        private bool isSelected;

        public T Value { get => this._value; set => Set(ref this._value, value); }
        public bool IsSelected
        {
            get => this.isSelected; set
            {
                Set(ref this.isSelected, value);
                SelectChanged?.Invoke(value);
            }
        }

        public Selectable(T value, bool isSelected = false, Action<bool> onSelectChanged = null)
        {
            this.Value = value;
            this.IsSelected = isSelected;
            SelectChanged += onSelectChanged;
        }

        public event Action<bool> SelectChanged;
    }
}
