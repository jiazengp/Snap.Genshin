using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DGP.Genshin.DataViewer.Helpers
{
    public class DirectoryEx
    {
        public static IEnumerable<FileEx> GetFileExs(string path) => Directory.GetFiles(path).Select(f => new FileEx(f));
    }
}
