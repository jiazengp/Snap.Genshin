using DGP.Genshin.Common.Data.Behavior;
using Newtonsoft.Json;

namespace DGP.Genshin.DataModel
{
    public class KeySource : Observable
    {
        [JsonIgnore] private bool isSelected = true;

        public string? Key { get; set; }
        public string? Source { get; set; }

        [JsonIgnore] public bool IsSelected { get => isSelected; set => Set(ref isSelected, value); }
    }
}