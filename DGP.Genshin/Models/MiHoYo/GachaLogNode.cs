using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo
{
    public class GachaLogNode
    {
        public GachaLogNode(string name, int count, IEnumerable<GachaLogNode> children)
        {
            this.Name = name;
            this.Count = count;
            this.Children = children;
        }

        public string Name { get; set; }
        public int Count { get; set; }
        public IEnumerable<GachaLogNode> Children { get; set; }
    }
}
