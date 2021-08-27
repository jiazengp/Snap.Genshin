using DGP.Snap.Framework.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
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

        // Timeout for performing the file download request.
        private static readonly int RequestTimeout = TimeSpan.FromSeconds(5).Milliseconds;

        static FileCache()
        {
            // default cache directory - can be changed if needed from App.xaml
            AppCacheDirectory = $"{Environment.CurrentDirectory}\\Cache\\";
        }

        public static string AppCacheDirectory { get; set; }

        public static async Task<MemoryStream> HitAsync(string url)
        {
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
            FileStream fileStream = null;
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

            WebRequest request = WebRequest.Create(uri);
            request.Timeout = RequestTimeout;
            try
            {
                WebResponse response = await request.GetResponseAsync();
                Stream responseStream = response.GetResponseStream();
                //url返回为空
                if (responseStream == null)
                    return null;
                //未在写文件
                if (!IsWritingFile.ContainsKey(fileName))
                {
                    IsWritingFile[fileName] = true;
                    fileStream = new FileStream(localFile, FileMode.Create, FileAccess.Write);
                }
                //开始写文件
                using (responseStream)
                {
                    byte[] bytebuffer = new byte[100];
                    int bytesRead;
                    do
                    {
                        //分段读取流内容
                        bytesRead = await responseStream.ReadAsync(bytebuffer, 0, 100);
                        if (fileStream != null)
                            await fileStream.WriteAsync(bytebuffer, 0, bytesRead);
                        await memoryStream.WriteAsync(bytebuffer, 0, bytesRead);
                    } while (bytesRead > 0);

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
            catch (WebException)
            {
                return null;
            }
        }
    }
}
