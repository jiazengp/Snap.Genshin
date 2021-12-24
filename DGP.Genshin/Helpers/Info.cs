using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DGP.Genshin.Helpers
{
    /// <summary>
    /// 在分析与崩溃中使用
    /// </summary>
    internal class Info
    {
        private Dictionary<string, string> analyticsInfo = new();
        public Info(string key,string value)
        {
            analyticsInfo[key] = value;
        }

        public Info(Type pageType, bool result)
        {
            analyticsInfo[pageType.ToString()] = result.ToString();
        }

        public IDictionary<string, string> Build()
        {
            return analyticsInfo;
        }

        private static string? userId;
        public static string UserId
        {
            get
            {
                userId ??= GetUniqueUserID();
                return userId;
            }
        }

        private static string GetUniqueUserID()
        {
            var UserName = Environment.UserName;
            var MachineGuid = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography\", "MachineGuid", UserName);
            var bytes = Encoding.UTF8.GetBytes(UserName + MachineGuid);
            var hash = MD5.Create().ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
