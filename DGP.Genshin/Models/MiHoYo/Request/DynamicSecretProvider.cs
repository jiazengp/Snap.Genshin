using DGP.Snap.Framework.Core.Logging;
using System;
using System.Security.Cryptography;
using System.Text;

namespace DGP.Genshin.Models.MiHoYo.Request
{
    /// <summary>
    /// 为MiHoYo接口请求器 <see cref="Requester"/> 提供动态密钥
    /// </summary>
    internal static class DynamicSecretProvider
    {
        public const string AppVersion = "2.10.1";

        private static readonly string APISalt = "4a8knnbk5pbjqsrudp3dq484m9axoc5g"; // @Azure99 respect original author
        public static string Create()
        {
            //unix timestamp
            int time = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            string random = GetRandomString(time);
            string check = GetComputedMd5($"salt={APISalt}&t={time}&r={random}");
            Logger.LogStatic(typeof(DynamicSecretProvider), $"generated DS:{time},{random},{check}");
            return $"{time},{random},{check}";
        }
        private static string GetRandomString(int time)
        {
            StringBuilder sb = new StringBuilder(6);
            Random random = new Random(time);

            for (int i = 0; i < 6; i++)
            {
                int v8 = random.Next(0, 32768) % 26;
                int v9 = 87;
                if (v8 < 10)
                {
                    v9 = 48;
                }
                sb.Append((char)(v8 + v9));
            }
            return sb.ToString();
        }
        private static string GetComputedMd5(string content)
        {
            using MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(content));

            StringBuilder builder = new StringBuilder();
            foreach (byte b in result)
                builder.Append(b.ToString("x2"));

            return builder.ToString();
        }
    }
}