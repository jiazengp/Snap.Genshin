using System;
using System.IO;

namespace DGP.Genshin.Data
{
    public static class StringExtensions
    {
        public static string? ToFileName(this string source)
        {
            if (!String.IsNullOrEmpty(source))
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