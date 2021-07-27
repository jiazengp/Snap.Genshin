using DGP.Genshin.Data.Helpers;
using Newtonsoft.Json;
using System.Windows.Media;

namespace DGP.Genshin.Data
{
    public abstract class Primitive : KeySource
    {
        public string Name { get; set; }
        public string Star { get; set; } = StarHelper.FromRank(1);
        [JsonIgnore] public SolidColorBrush StarSolid => StarHelper.ToSolid(this.Star);
    }
}
