using DGP.Genshin.DataModels.Helpers;
using Newtonsoft.Json;
using System.Windows.Media;

namespace DGP.Genshin.DataModels
{
    public abstract class Primitive : KeySource
    {
        public string? Name { get; set; }
        public string? Star { get; set; } = StarHelper.FromRank(1);
        [JsonIgnore] public SolidColorBrush? StarSolid => StarHelper.ToSolid(Star);
    }
}
