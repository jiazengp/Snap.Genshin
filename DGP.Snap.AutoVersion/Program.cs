using System;
using System.IO;

namespace DGP.Snap.AutoVersion
{
    /// <summary>
    /// 编译时，项目版本号自动更新的工具
    /// <para>使用方法：</para>
    /// <para>打开要处理的项目，对其属性下的生成事件页签，预生成事件命令行输入</para>
    /// <para><code>$(SolutionDir)Build\net5.0\DGP.Snap.AutoVersion.exe "$(ProjectDir)"</code></para>
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("没有设置参数，应该要设为：Snap.AutoVersion \"$(ProjectDir)\"");
                return;
            }
            else if (args.Length > 1)
            {
                Console.WriteLine("工具有多个参数，是不是你的命令行没带双引号，而项目路径又带空格？");
                return;
            }

            string sPath = args[0].Replace("\"", String.Empty) + "\\Properties\\";
            string sAssemOld = sPath + "AssemblyInfo.old";
            string sAssem = sPath + "AssemblyInfo.cs";
            string sAssemNew = sPath + "AssemblyInfo.new";

            // 检测AssemblyInfo文件是否存在来决定路径是否设置正确
            if (File.Exists(sAssem) == false)
            {
                string sInfo = "未检测到文件存在：" + sAssem + "\r\n" + "路径是否有设置错误？";
                Console.WriteLine(sInfo);
                return;
            }

            using (StreamReader sr = new StreamReader(sAssem))
            {
                using StreamWriter sw = new StreamWriter(sAssemNew, false, sr.CurrentEncoding);
                string line;
                string newLine;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.IndexOf("[assembly: AssemblyVersion") == 0)
                    {
                        // 找到这两行了，修改它们
                        newLine = GetTargetVersion(line);
                    }
                    else
                    {
                        newLine = line;
                    }
                    sw.WriteLine(newLine);
                }
            }

            // 备份文件删除（只读属性时将会出错）
            if (File.Exists(sAssemOld) == true)
            {
                File.Delete(sAssemOld);
            }

            // 原文件改名备份（只读属性下允许正常改名）
            File.Move(sAssem, sAssemOld);
            // 新文件改为原文件（原只读属性将会丢失）
            File.Move(sAssemNew, sAssem);
        }

        /// <summary>
        /// 对输入的字符串, 取其中版本最后部分+1
        /// </summary>
        /// <param name="sLine">输入的字符串，类似：[assembly: AssemblyVersion("1.0.0.4")]</param>
        /// <returns>版本最后部分+1 后的结果</returns>
        private static string GetTargetVersion(string sLine)
        {
            // 定位起始位置与结束位置
            int posStart = sLine.IndexOf("(\"");
            if (posStart < 0)
            {
                Console.WriteLine("该字符串找不到版本号起始标志\"：" + sLine);
                Environment.Exit(0);
            }
            int posEnd = sLine.IndexOf("\")", posStart);
            if (posEnd < 0)
            {
                Console.WriteLine("该字符串找不到版本号结束标志\"：" + sLine);
                Environment.Exit(0);
            }

            string sVer = sLine.Substring(posStart + 2, posEnd - posStart - 2);
            VersionEx currentVersion = new VersionEx(sVer);

            DateTime now = DateTime.Now;
            VersionEx newVersion = new VersionEx(now.Year, now.Month, now.Day, currentVersion.Revision);
            if (newVersion > currentVersion)
            {
                newVersion.Revision = 1;
            }
            else
            {
                newVersion.Revision = currentVersion.Revision + 1;
            }

            return "[assembly: AssemblyVersion(\"" + newVersion + "\")]";
        }
    }
}
