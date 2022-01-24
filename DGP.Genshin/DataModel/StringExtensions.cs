using System;
using System.IO;

namespace DGP.Genshin.DataModel
{
    public static class StringExtensions
    {
        /// <summary>
        /// 获取url中的文件名部分
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string? ToFileName(this string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (Uri.TryCreate(source, UriKind.Absolute, out Uri? uri))
                {
                    return Path.GetFileName(uri.LocalPath);
                }
            }
            return null;
        }
    }
}