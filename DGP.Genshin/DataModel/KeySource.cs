using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DGP.Genshin.DataModel
{
    /// <summary>
    /// 键源对
    /// 同时提供了 <see cref="IsSelected"/> 已选中 属性,以支持筛选
    /// </summary>
    public class KeySource : ObservableObject
    {
        public KeySource()
        {

        }

        public KeySource(string key, string source)
        {
            Key = key;
            Source = source;
        }
        public string? Key { get; set; }
        public string? Source { get; set; }

        #region Observable
        private bool isSelected = true;
        public bool IsSelected
        {
            get => isSelected;

            set => SetProperty(ref isSelected, value);
        }
        #endregion
    }
}