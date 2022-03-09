using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DGP.Genshin.DataModel.Promotion
{
    public class MaterialList : ObservableCollection<CalculableConsume>
    {
        public MaterialList(IEnumerable<CalculableConsume> items) : base(items)
        {

        }
    }
}
