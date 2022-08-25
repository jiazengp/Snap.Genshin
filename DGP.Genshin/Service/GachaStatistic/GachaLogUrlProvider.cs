using DGP.Genshin.Control.GenshinElement.GachaStatistic;
using DGP.Genshin.Service.Abstraction.GachaStatistic;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using ModernWpf.Controls;
using Snap.Data.Primitive;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace DGP.Genshin.Service.GachaStatistic
{
    /// <summary>
    /// 联机抽卡Url提供器
    /// </summary>
    internal static class GachaLogUrlProvider
    {
        private const string GachaLogBaseUrl = "https://hk4e-api.mihoyo.com/event/gacha_info/api/getGachaLog";
        private const string WebStaticHost = @"https://webstatic.mihoyo.com";
        private const string Hk4eApiHost = @"https://hk4e-api.mihoyo.com";

        private static readonly string LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static readonly string LogFilePath = $@"{LocalPath}Low\miHoYo\原神\output_log.txt";

        /// <summary>
        /// 根据模式获取Url
        /// </summary>
        /// <param name="mode">模式</param>
        /// <returns>文件存在返回true,若获取失败返回null</returns>
        public static async Task<Result<bool, string>> GetUrlAsync(GachaLogUrlMode mode)
        {
            switch (mode)
            {
                case GachaLogUrlMode.Proxy:
                    string url = string.Empty;
                    if (File.Exists(LogFilePath))
                    {
                        url = await GetUrlFromProxyAsync();
                    }

                    bool isOk = url != string.Empty;
                    return new(isOk, url);
                case GachaLogUrlMode.ManualInput:
                    return await GetUrlFromManualInputAsync();
                default:
                    throw Must.NeverHappen();
            }
        }

        private static async Task<string> GetUrlFromProxyAsync()
        {
            ContentDialog blockingDialog = new()
            {
                Title = "获取抽卡链接中",
                Content = "请打开游戏 [祈愿] 功能中的 [历史记录] 页面\n关闭主界面可能导致崩溃,请勿轻易尝试",
            };
            using (blockingDialog.BlockInteraction())
            {
                using (ProxyHelper proxyHelper = new())
                {
                    string url = await proxyHelper.GetTargetUrlAsync();

                    url = url.Replace("#/log", string.Empty);
                    string[] splitedUrl = url.Split('?');
                    splitedUrl[0] = GachaLogBaseUrl;
                    string result = string.Join("?", splitedUrl);

                    return result;
                }
            }
        }

        /// <summary>
        /// 获取用户输入的Url
        /// </summary>
        /// <returns>用户输入的Url，若不可用则为 null</returns>
        private static async Task<Result<bool, string>> GetUrlFromManualInputAsync()
        {
            Result<bool, string> input = await new GachaLogUrlDialog().GetInputUrlAsync();
            string result = string.Empty;
            if (input.IsOk)
            {
                string url = input.Value.Trim();

                // compat with iOS url
                if (url.StartsWith(WebStaticHost) || url.StartsWith(Hk4eApiHost))
                {
                    url = url.Replace("#/log", string.Empty);
                    string[] splitedUrl = url.Split('?');
                    splitedUrl[0] = GachaLogBaseUrl;
                    result = string.Join("?", splitedUrl);
                }
            }

            return new(input.IsOk, result);
        }
    }
}

/// <summary>
/// 代理帮助类
/// </summary>
internal class ProxyHelper : IDisposable
{
    private readonly ProxyServer proxyServer;
    private readonly TaskCompletionSource probingUrl = new();

    private string? targetUrl;

    /// <summary>
    /// 构造一个新的代理帮助类
    /// </summary>
    public ProxyHelper()
    {
        proxyServer = new ProxyServer();
        proxyServer.BeforeRequest += OnEventAsync;

        ExplicitProxyEndPoint endPoint = new(IPAddress.Any, 18371);
        proxyServer.AddEndPoint(endPoint);
        proxyServer.Start();
        proxyServer.SetAsSystemProxy(endPoint, ProxyProtocolType.AllHttp);
    }

    /// <summary>
    /// 获取Url
    /// </summary>
    /// <returns>url</returns>
    public async Task<string> GetTargetUrlAsync()
    {
        await probingUrl.Task;
        return targetUrl ?? string.Empty;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        proxyServer.Stop();
        proxyServer.DisableAllSystemProxies();
        proxyServer.Dispose();
    }

    private Task OnEventAsync(object sender, SessionEventArgs e)
    {
        Titanium.Web.Proxy.Http.Request request = e.HttpClient.Request;
        if (request.Host == "webstatic.mihoyo.com" && request.RequestUri.AbsolutePath == "/hk4e/event/e20190909gacha-v2/index.html")
        {
            targetUrl = request.RequestUriString;
            probingUrl.TrySetResult();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// 对话框扩展
/// </summary>
internal static class ContentDialogExtensions
{
    /// <summary>
    /// 阻止用户交互
    /// </summary>
    /// <param name="contentDialog">对话框</param>
    /// <returns>用于恢复用户交互</returns>
    public static IDisposable BlockInteraction(this ContentDialog contentDialog)
    {
        contentDialog.ShowAsync().Forget();
        return new ContentDialogHider(contentDialog);
    }

    private struct ContentDialogHider : IDisposable
    {
        private readonly ContentDialog contentDialog;

        public ContentDialogHider(ContentDialog contentDialog)
        {
            this.contentDialog = contentDialog;
        }

        public void Dispose()
        {
            contentDialog.Hide();
        }
    }
}