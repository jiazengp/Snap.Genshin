using DGP.Genshin.Control.GenshinElement.GachaStatistic;
using DGP.Genshin.Service.Abstratcion;
using Snap.Exception;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DGP.Genshin.Service.GachaStatistic
{
    /// <summary>
    /// 联机抽卡Url提供器
    /// </summary>
    internal class GachaLogUrlProvider
    {
        private const string gachaLogBaseUrl = "https://hk4e-api.mihoyo.com/event/gacha_info/api/getGachaLog";
        private static readonly string LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static readonly string logFilePath = $@"{LocalPath}Low\miHoYo\原神\output_log.txt";

        /// <summary>
        /// 防止从外部创建实例
        /// </summary>
        private GachaLogUrlProvider() { }

        /// <summary>
        /// 根据模式获取Url
        /// </summary>
        /// <param name="mode">模式</param>
        /// <returns>文件存在返回true,若获取失败返回null</returns>
        public static async Task<(bool isOk, string? url)> GetUrlAsync(GachaLogUrlMode mode)
        {
            switch (mode)
            {
                case GachaLogUrlMode.GameLogFile:
                    bool filePresent = File.Exists(logFilePath);
                    return (filePresent, filePresent ? await GetUrlFromLogFileAsync() : null);
                case GachaLogUrlMode.ManualInput:
                    return await GetUrlFromManualInputAsync();
                default:
                    throw new SnapGenshinInternalException("switch 分支不应命中 default");
            }
        }

        /// <summary>
        /// 在日志文件中寻找url
        /// </summary>
        /// <returns></returns>
        private static async Task<string?> GetUrlFromLogFileAsync()
        {
            string? result = null;
            //share the file to make unity access it so it doesn't crash when game is running
            using (StreamReader sr = new(File.Open(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                string? str;
                //check till the log file end to make sure
                while (sr.Peek() >= 0)
                {
                    str = await sr.ReadLineAsync();
                    if (str is not null && str.StartsWith("OnGetWebViewPageFinish:") && str.EndsWith("#/log"))
                    {
                        str = str.Replace("#/log", "");
                        string[] splitedUrl = str.Split('?');
                        splitedUrl[0] = gachaLogBaseUrl;
                        result = string.Join("?", splitedUrl);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取用户输入的Url
        /// </summary>
        /// <returns>用户输入的Url，若不可用则为 null</returns>
        private static async Task<(bool isOk, string? url)> GetUrlFromManualInputAsync()
        {
            (bool isOk, string url) = await new GachaLogUrlDialog().GetInputUrlAsync();
            url = url.Trim();
            string? result = null;
            //compat with iOS url
            if (url.StartsWith(@"https://webstatic.mihoyo.com"))
            {
                url = url.Replace("#/log", "");
                string[] splitedUrl = url.Split('?');
                splitedUrl[0] = gachaLogBaseUrl;
                result = string.Join("?", splitedUrl);
            }
            return (isOk, result);
        }
    }
}