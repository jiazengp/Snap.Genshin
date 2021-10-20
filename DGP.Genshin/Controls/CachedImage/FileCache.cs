using DGP.Snap.Framework.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DGP.Genshin.Controls.CachedImage
{
    [Github("https://github.com/floydpink/CachedImage/blob/main/source/FileCache.cs")]
    public static class FileCache
    {
        public enum CacheMode
        {
            Dedicated
        }

        // Record whether a file is being written.
        private static readonly Dictionary<string, bool> IsWritingFile = new Dictionary<string, bool>();

        static FileCache()
        {
            // default cache directory - can be changed if needed from App.xaml
            AppCacheDirectory = $"{Environment.CurrentDirectory}\\Cache\\";
        }

        public static string AppCacheDirectory { get; set; }

        [SuppressMessage("", "CA5350")]
        [SuppressMessage("", "CA1304")]
        [SuppressMessage("", "CA1835")]
        public static async Task<MemoryStream?> HitAsync(string? url)
        {
            if (url is null)
            {
                return null;
            }
            Directory.CreateDirectory(AppCacheDirectory);

            Uri uri = new Uri(url);
            //build filename
            StringBuilder fileNameBuilder = new StringBuilder();
            using (SHA1Managed sha1 = new())
            {
                string canonicalUrl = uri.ToString();
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(canonicalUrl));
                fileNameBuilder.Append(BitConverter.ToString(hash).Replace("-", "").ToLower());
                if (Path.HasExtension(canonicalUrl))
                    fileNameBuilder.Append(Path.GetExtension(canonicalUrl).Split('?')[0]);
            }
            string fileName = fileNameBuilder.ToString();
            string localFile = $"{AppCacheDirectory}\\{fileName}";

            MemoryStream memoryStream = new MemoryStream();
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

            HttpClient client = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            try
            {
                HttpResponseMessage? response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode is false)
                {
                    return null;
                }

                Stream? responseStream = await response.Content.ReadAsStreamAsync();

                if (!IsWritingFile.ContainsKey(fileName))
                {
                    IsWritingFile[fileName] = true;
                    fileStream = new FileStream(localFile, FileMode.Create, FileAccess.Write);
                }

                using (responseStream)
                {
                    byte[]? bytebuffer = new byte[100];
                    int bytesRead;
                    do
                    {
                        bytesRead = await responseStream.ReadAsync(bytebuffer, 0, 100);
                        if (fileStream != null)
                            await fileStream.WriteAsync(bytebuffer, 0, bytesRead);
                        await memoryStream.WriteAsync(bytebuffer, 0, bytesRead);
                    }
                    while (bytesRead > 0);
                    if (fileStream != null)
                    {
                        await fileStream.FlushAsync();
                        fileStream.Dispose();
                        IsWritingFile.Remove(fileName);
                    }
                }
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            catch (Exception ex) when (ex is WebException || ex is IOException || ex is HttpRequestException)
            {
                return null;
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}
