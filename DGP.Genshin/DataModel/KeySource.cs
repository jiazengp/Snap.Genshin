using DGP.Genshin.Common.Data.Behavior;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace DGP.Genshin.DataModel
{
    public class KeySource : ObservableObject
    {
        public string? Key { get; set; }
        public string? Source { get; set; }

        [JsonIgnore] private bool isSelected = true;
        [JsonIgnore] public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
    }
}