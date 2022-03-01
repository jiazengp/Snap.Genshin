using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace DGP.Genshin.DataModel
{
    /// <summary>
    /// 键源对
    /// 同时提供了<see cref="IsSelected"/>已选中属性,以支持筛选
    /// </summary>
    public class KeySource : ObservableObject
    {
        public string? Key { get; set; }
        public string? Source { get; set; }

        [JsonIgnore] private bool isSelected = true;
        [JsonIgnore]
        public bool IsSelected
        {
            get => isSelected;

            set => SetProperty(ref isSelected, value);
        }
    }
}