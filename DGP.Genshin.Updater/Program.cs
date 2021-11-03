using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace DGP.Genshin.Updater
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            EnsureWorkingPath();
            bool hasArgs = args.Length > 0 && args[0] == "UpdateInstall";
            bool hasPackage = File.Exists("../Package.zip");

            switch ((hasArgs, hasPackage))
            {
                case (true, true):
                    ExtractPackage();
                    break;
                case (false, true):
                    ManualUpdate();
                    break;
                case (_, false):
                    Console.WriteLine("未找到更新文件");
                    Console.ReadKey();
                    break;
            };
        }

        private static void ManualUpdate()
        {
            Console.WriteLine("检测到安装包存在，是否手动安装？(Y/其他任何按键)");
            ConsoleKeyInfo key = Console.ReadKey();
            if (key.Key is ConsoleKey.Y)
            {
                ExtractPackage();
            }
        }

        private static void ExtractPackage()
        {
            Console.WriteLine("准备开始安装...");
            Process[] ps = Process.GetProcessesByName("DGP.Genshin");
            if (ps.Length > 0)
            {
                Process p = ps[0];
                p.CloseMainWindow();
                Console.WriteLine("等待 Snap Genshin 退出中...(可能需要手动退出)");
                p.WaitForExit();
            }
            Console.Clear();
            using (ZipArchive archive = ZipFile.OpenRead("../Package.zip"))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    try
                    {
                        if (entry.FullName.EndsWith("/"))
                        {
                            string directoryName = entry.FullName.Substring(0, entry.FullName.Length - 1);
                            Console.WriteLine($"创建目录:{directoryName}");
                            Directory.CreateDirectory($"../{directoryName}");
                        }
                        else
                        {
                            Console.WriteLine($"提取文件:{entry.FullName}");
                            entry.ExtractToFile($"../{entry.FullName}", true);
                        }
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            Console.WriteLine("正在启动Snap Genshin");
            Process.Start("../DGP.Genshin.exe");
        }
        /// <summary>
        /// set working dir while launch by windows autorun
        /// </summary>
        private static void EnsureWorkingPath()
        {
            string? path = Assembly.GetEntryAssembly()?.Location;
            string? workingPath = Path.GetDirectoryName(path);
            if (workingPath is not null)
            {
                Environment.CurrentDirectory = workingPath;
            }
        }
    }
}
