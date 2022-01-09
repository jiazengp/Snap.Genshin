using System;
using System.IO;

namespace DGP.Genshin.Helpers
{
    /// <summary>
    /// Snap Genshin 文件路径解析上下文
    /// 不能用于处理其他位置的文件
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

        /// <summary>
        /// 创建文件，若已存在文件，则不会创建
        /// </summary>
        /// <param name="file"></param>
        internal static void CreateOrIgnore(string file)
        {
            file = Locate(file);
            if (!File.Exists(file))
            {
                File.Create(file).Dispose();
            }
        }
    }
    internal enum MoveOperation
    {

    }
}
