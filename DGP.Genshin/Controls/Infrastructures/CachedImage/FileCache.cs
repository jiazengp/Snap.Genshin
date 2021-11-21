using DGP.Genshin.Common.Core.Logging;
using DGP.Genshin.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DGP.Genshin.Controls.Infrastructures.CachedImage
{
    /// <summary>
    /// https://github.com/floydpink/CachedImage/blob/main/source/FileCache.cs
    /// </summary>
    public static class FileCache
    {
        // Record whether a file is being written.
        private static readonly Dictionary<string, bool> IsWritingFile = new();

        public static string AppCacheDirectory { get; set; } = $"{Environment.CurrentDirectory}\\Cache\\";

        // HttpClient is intended to be instantiated once per application, rather than per-use.
        private static readonly Lazy<HttpClient> LazyHttpClient = new(() => new() { Timeout = Timeout.InfiniteTimeSpan });

        /// <summary>
        /// Url命中
        /// </summary>
        /// <param name="url"></param>
        /// <returns>缓存或下载的图片</returns>
        public static async Task<MemoryStream?> HitAsync(string? url)
        {
            if (url is null)
            {
                return null;
            }

            Directory.CreateDirectory(AppCacheDirectory);

            Uri uri = new(url);
            string fileName = BuildFileName(uri);
            string localFile = $"{AppCacheDirectory}\\{fileName}";

            MemoryStream memoryStream = new();
            FileStream? fileStream = null;
            //未写文件且文件存在
            //读取文件缓存并返回
            if (!IsWritingFile.ContainsKey(fileName) && File.Exists(localFile))
            {
                using (fileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }

            HttpClient client = LazyHttpClient.Value;
            try
            {
                HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                Stream? responseStream = await response.Content.ReadAsStreamAsync();

                if (!IsWritingFile.ContainsKey(fileName))
                {
                    IsWritingFile[fileName] = true;
                    fileStream = new FileStream(localFile, FileMode.Create, FileAccess.Write);
                }

                await CopyToCacheAndMemoryAsync(responseStream, memoryStream, fileStream, fileName);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            catch (Exception ex)
            {
                memoryStream?.Dispose();
                fileStream?.Dispose();
                File.Delete(localFile);
                Logger.LogStatic($"Caching {url} To {fileName} failed.File has deleted.");
                Logger.LogStatic(ex);
                return null;
            }
        }

        [SuppressMessage("", "CA5350")]
        private static string BuildFileName(Uri uri)
        {
            StringBuilder fileNameBuilder = new();
            using (SHA1 sha1 = SHA1.Create())
            {
                string canonicalUrl = uri.ToString();
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(canonicalUrl));
                fileNameBuilder.Append(BitConverter.ToString(hash).Replace("-", "").ToLower(CultureInfoHelper.Default));
                if (Path.HasExtension(canonicalUrl))
                {
                    fileNameBuilder.Append(Path.GetExtension(canonicalUrl).Split('?')[0]);
                }
            }
            string fileName = fileNameBuilder.ToString();
            return fileName;
        }

        [SuppressMessage("", "CA1835")]
        private static async Task CopyToCacheAndMemoryAsync(Stream response, MemoryStream memory, FileStream? file, string fileName)
        {
            using (response)
            {
                byte[] buffer = new byte[100];
                int bytesRead;
                do
                {
                    bytesRead = await response.ReadAsync(buffer, 0, 100);
                    if (file is not null)
                    {
                        await file.WriteAsync(buffer, 0, bytesRead);
                    }
                    await memory.WriteAsync(buffer, 0, bytesRead);
                }
                while (bytesRead > 0);

                if (file is not null)
                {
                    await file.FlushAsync();
                    file.Dispose();
                    IsWritingFile.Remove(fileName);
                }
            }
        }
    }
}
