using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Resin.Plugin.ViewModel;
using Microsoft.Win32;
using Snap.Core.Logging;
using Snap.Data.Utility;
using Snap.Exception;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

[assembly: SnapGenshinPlugin]

namespace DGP.Genshin.Resin.Plugin
{
    [ImportPage(typeof(ResinConfigPage),"桌面树脂","\uF182")]
    public class ResinPlugin : IPlugin
    {
        private const string TokenFile = "Resin.token";
        public bool IsEnabled { get; set; }
        public string Name { get =>"实时树脂小组件"; }
        public string Description { get => "实时树脂小组件现在重新回到 Snap Genshin!"; }
        public string Author { get => "DGP Studio"; }
        public Version Version { get => new("1.0.0.0"); }

        public ResinPlugin()
        {
            if (File.Exists(TokenFile))
            {
                string token = File.ReadAllText(TokenFile);
                if(Base64Converter.Base64Encode(Encoding.UTF8,User.Id) == token)
                {
                    this.Log("成功加载");
                    App.Current.Startup += CurrentStartup;
                    return;
                }
                else
                {
                    throw new SnapGenshinInternalException("token文件与你的计算机不匹配，请在赞助开发者后联系重新获取");
                }
            }
            else
            {
                throw new SnapGenshinInternalException("未在程序根目录找到token文件，请在赞助开发者后联系获取");
            }
        }

        private void CurrentStartup(object sender, System.Windows.StartupEventArgs e)
        {
            MethodInfo[]? methods = typeof(App).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
            foreach (MethodInfo? method in methods)
            {
                if (method.Name == "AutoWired" && (!method.IsGenericMethod))
                {
                    try
                    {
                        _ = method.Invoke(App.Current, new[] { typeof(ResinConfigViewModel) });
                        this.Log("ResinConfigViewModel initialized");
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 用户信息
        /// </summary>
        internal class User
        {
            private const string CryptographyKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography\";
            private const string MachineGuidValue = "MachineGuid";

            private static string? userId;
            public static string Id
            {
                get
                {
                    userId ??= GetUniqueUserID();
                    return userId;
                }
            }

            private static string GetUniqueUserID()
            {
                string UserName = Environment.UserName;
                object? MachineGuid = Registry.GetValue(CryptographyKey, MachineGuidValue, UserName);
                byte[] bytes = Encoding.UTF8.GetBytes(UserName + MachineGuid);
                byte[] hash = MD5.Create().ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}