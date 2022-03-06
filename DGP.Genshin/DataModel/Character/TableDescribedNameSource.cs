using System.Collections.Generic;

namespace DGP.Genshin.DataModel.Character
{
    public class TableDescribedNameSource : DescribedNameSource
    {
        public List<NameValues<SkillStatValues>>? Table { get; set; }
    }
}
