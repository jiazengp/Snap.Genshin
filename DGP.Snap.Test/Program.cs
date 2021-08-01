using System.Diagnostics;
using System.IO;

namespace DGP.Snap.Test
{
    internal class Program
    {
        private static void Main(string[] args) => Debug.WriteLine(Directory.Exists(@"Metadata"));
    }
}
