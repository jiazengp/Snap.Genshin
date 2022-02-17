using DGP.Genshin.DataModel.Helper;
using Newtonsoft.Json;
using System.Windows.Media;

namespace DGP.Genshin.DataModel
{
    /// <summary>
    /// 物品元件，在 <see cref="KeySource"/> 的基础上增加了名称与星级
    /// </summary>
    public abstract class Primitive : KeySource
    {
        public string? Name { get; set; }
        public string? Star { get; set; } = StarHelper.FromInt32Rank(1);
        [JsonIgnore] public SolidColorBrush? StarSolid => StarHelper.ToSolid(Star);

        public virtual string? GetBadge()
        {
            return null;
        }
    }
}
