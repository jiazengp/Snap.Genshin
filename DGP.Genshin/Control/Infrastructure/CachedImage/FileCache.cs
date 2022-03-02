using Snap.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DGP.Genshin.Control.Infrastructure.CachedImage
{
    /// <summary>
    /// https://github.com/floydpink/CachedImage/blob/main/source/FileCache.cs
    /// </summary>
    internal static class FileCache
    {
        // Record whether a file is being written.
        private static readonly Dictionary<string, bool> IsWritingFile = new();

        public static string AppCacheDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "Cache");

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
                Logger.LogStatic($"Download {uri} as {fileName}");
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
                Logger.LogStatic($"Download {uri} completed.");
                return memoryStream;
            }
            catch (Exception ex)
            {
                await memoryStream.DisposeAsync();
                if (fileStream is not null)
                {
                    await fileStream.DisposeAsync();
                }
                File.Delete(localFile);
                Logger.LogStatic($"Caching {url} To {fileName} failed.File has deleted.");
                Logger.LogStatic(ex);
                return null;
            }
        }
        /// <summary>
        /// 用于图片缓存资源验证
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool Exists(string? url)
        {
            if (url is null)
            {
                return true;
            }

            Directory.CreateDirectory(AppCacheDirectory);

            Uri uri = new(url);
            string fileName = BuildFileName(uri);
            string localFile = $"{AppCacheDirectory}\\{fileName}";
            return File.Exists(localFile);
        }

        /// <summary>
        /// 构造缓存文件名称
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static string BuildFileName(Uri uri)
        {
            StringBuilder fileNameBuilder = new();
            using (SHA1 sha1 = SHA1.Create())
            {
                string canonicalUrl = uri.ToString();
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(canonicalUrl));
                fileNameBuilder.Append(BitConverter.ToString(hash).Replace("-", "").ToLower(CultureInfo.DefaultThreadCurrentCulture));
                if (Path.HasExtension(canonicalUrl))
                {
                    fileNameBuilder.Append(Path.GetExtension(canonicalUrl).Split('?')[0]);
                }
            }
            string fileName = fileNameBuilder.ToString();
            return fileName;
        }

        /// <summary>
        /// 复制到缓存与文件
        /// </summary>
        /// <param name="response"></param>
        /// <param name="memory"></param>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [SuppressMessage("", "CA1835")]
        private static async Task CopyToCacheAndMemoryAsync(Stream response, MemoryStream memory, FileStream? file, string fileName)
        {
            using (response)
            {
                const int bufferSize = 128;
                byte[] buffer = new byte[bufferSize];
                int bytesRead;
                do
                {
                    bytesRead = await response.ReadAsync(buffer, 0, bufferSize);
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
                    await file.DisposeAsync();
                    IsWritingFile.Remove(fileName);
                }
            }
        }
    }
}
