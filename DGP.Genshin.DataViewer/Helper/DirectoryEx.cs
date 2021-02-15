using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.DataViewer.Helper
{
    public class DirectoryEx
    {
        public static IEnumerable<FileEx> GetFileExs(string path)
        {
            return Directory.GetFiles(path).Select(f => new FileEx(f));
        }
    }
}
