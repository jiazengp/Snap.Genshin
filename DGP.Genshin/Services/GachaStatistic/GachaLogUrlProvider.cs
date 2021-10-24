using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.GachaStatistic
{
    /// <summary>
    /// 联机抽卡Url提供器
    /// </summary>
    public class GachaLogUrlProvider
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
        /// <returns>若获取失败返回null</returns>
        public static async Task<string?> GetUrlAsync(GachaLogUrlMode mode)
        {
            return mode switch
            {
                GachaLogUrlMode.GameLogFile => File.Exists(logFilePath) ? await GetUrlFromLogFileAsync() : null,
                GachaLogUrlMode.ManualInput => await GetUrlFromManualInputAsync(),
                _ => null,
            };
        }

        /// <summary>
        /// 在日志文件中寻找url
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("", "CA1310")]
        private static async Task<string?> GetUrlFromLogFileAsync()
        {
            string? result = null;
            //share the file to make genshin access it so it doesn't crash when game is running
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
        [SuppressMessage("", "CA1310")]
        private static async Task<string?> GetUrlFromManualInputAsync()
        {
            string str = await new GachaLogUrlDialog().GetInputUrlAsync();
            str = str.Trim();
            string? result = null;
            if (str.StartsWith(@"https://webstatic.mihoyo.com") && str.EndsWith("#/log"))
            {
                str = str.Replace("#/log", "");
                string[] splitedUrl = str.Split('?');
                splitedUrl[0] = gachaLogBaseUrl;
                result = string.Join("?", splitedUrl);
            }
            return result;
        }
    }
}
