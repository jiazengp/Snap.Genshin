using DGP.Genshin.Helpers;
using DGP.Snap.SGInstaller.Helper;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DGP.Snap.SGInstaller
{
    public class Program
    {
        private const string NETRuntime = "https://download.visualstudio.microsoft.com/download/pr/bf058765-6f71-4971-aee1-15229d8bfb3e/c3366e6b74bec066487cd643f915274d/windowsdesktop-runtime-6.0.1-win-x64.exe";
        private const string WebView2Runtime = "https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.1-windows-x64-installer";

        public static async Task Main(string[] args)
        {
            Console.WriteLine("欢迎使用 Snap Genshin 运行环境安装工具");
            Console.WriteLine("是否开始安装 Snap Genshin 所需的环境? [Y 继续/N 退出]");
            if (!ConfirmUserAction())
            {
                Environment.Exit(0);
            }
            Console.WriteLine("\n下载 .NET 运行时? [Y 是/N 跳过]");
            if (ConfirmUserAction())
            {
                await DownloadAndInstallNET6RuntimeAsync();
            }

            Console.WriteLine("\n下载 WebView2 运行时? [Y 是/N 跳过]");
            if (ConfirmUserAction())
            {
                await DownloadAndInstallWebView2RuntimeAsync();
            }

            Console.WriteLine("\n安装已完成，按任意键退出");
            Console.ReadKey();
        }
        private static bool ConfirmUserAction()
        {
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.Y:
                        return true;
                    case ConsoleKey.N:
                        return false;
                    default:
                        break;
                }
            }
        }

        private static async Task DownloadAndInstallNET6RuntimeAsync()
        {
            string installerPath = PathContext.Locate("net-installer.exe");
            Downloader downloader = new(NETRuntime, installerPath);
            downloader.ProgressChanged += (tb, br, p) => Console.WriteLine($@"{p:P2} - {br * 1.0 / 1024 / 1024:F2}MB / {tb * 1.0 / 1024 / 1024:F2}MB");
            
            bool caught = false;
            try
            {
                await downloader.DownloadAsync();
            }
            catch
            {
                caught = true;
            }
            if (!caught)
            {
                Process installer = new()
                {
                    StartInfo = new()
                    {
                        FileName = installerPath,
                        Verb = "runas",
                        UseShellExecute = true,
                        Arguments = new CommandLine().With("/install").With("/norestart").Build()
                    }
                };
                Console.WriteLine("\n开始安装 .NET 运行时");
                installer.Start();
                await installer.WaitForExitAsync();
                switch (installer.ExitCode)
                {
                    case 0:
                        Console.WriteLine("安装完成\n");
                        break;
                    case 3010:
                        Console.WriteLine("安装完成，稍后需要重启\n");
                        break;
                    default:
                        Console.WriteLine("安装失败\n");
                        break;
                }
            }
            else
            {
                Console.WriteLine("\n下载失败，请重试");
            }
        }

        private static async Task DownloadAndInstallWebView2RuntimeAsync()
        {
            string installerPath = PathContext.Locate("webview2-installer.exe");
            Downloader downloader = new(WebView2Runtime, installerPath);
            downloader.ProgressChanged += (tb, br, p) => Console.WriteLine($@"{p:P2} - {br * 1.0 / 1024 / 1024:F2}MB / {tb * 1.0 / 1024 / 1024:F2}MB");

            bool caught = false;
            try
            {
                await downloader.DownloadAsync();
            }
            catch
            {
                caught = true;
            }
            if (!caught)
            {
                Process installer = new()
                {
                    StartInfo = new()
                    {
                        FileName = installerPath,
                        Verb = "runas",
                        UseShellExecute = true,
                        Arguments = new CommandLine().With("/install").With("/norestart").Build()
                    }
                };
                Console.WriteLine("\n开始安装 WebView2 运行时");
                installer.Start();
                await installer.WaitForExitAsync();
                switch (installer.ExitCode)
                {
                    case 0:
                        Console.WriteLine("安装完成\n");
                        break;
                    case 3010:
                        Console.WriteLine("安装完成，稍后需要重启\n");
                        break;
                    default:
                        Console.WriteLine("安装失败\n");
                        break;
                }
            }
            else
            {
                Console.WriteLine("\n下载失败，请重试");
            }
        }
    }
}

