using System.Linq;

namespace DGP.Genshin.Helper.Extension
{
    public static class BytesExtensions
    {
        public static string Stringify(this byte[] bytes)
        {
            return string.Concat(bytes.Select(b => b.ToString("X2")));
        }
    }
}
