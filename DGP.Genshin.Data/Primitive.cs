using System.Windows.Media;

namespace DGP.Genshin.Data
{
    public abstract class Primitive
    {
        public string Name { get; set; }
        public int Star { get; set; } = 1;
        public string Source { get; set; }
    }
}
