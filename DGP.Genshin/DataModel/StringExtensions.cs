using System;
using System.IO;

namespace DGP.Genshin.DataModels
{
    public static class StringExtensions
    {
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