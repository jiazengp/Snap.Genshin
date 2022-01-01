using System;
using System.IO;

namespace DGP.Genshin.Helpers
{
    /// <summary>
    /// 路径解析上下文
    /// </summary>
    internal static class PathContext
    {
        /// <summary>
        /// 定位子文件夹中的文件
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string Locate(string folder, string file)
        {
            return Path.GetFullPath(Path.Combine(folder, file), AppContext.BaseDirectory);
        }
    }
}
