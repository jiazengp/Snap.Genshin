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
        /// 定位根目录中的文件
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        internal static string Locate(string file)
        {
            return Path.GetFullPath(file, AppContext.BaseDirectory);
        }
        /// <summary>
        /// 定位根目录下子文件夹中的文件
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        internal static string Locate(string folder, string file)
        {
            return Path.GetFullPath(Path.Combine(folder, file), AppContext.BaseDirectory);
        }

        internal static void MoveTo()
        {

        }
    }
    internal enum MoveOperation
    {

    }
}
