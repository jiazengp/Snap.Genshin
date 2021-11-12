using Microsoft.Win32;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Launching
{
    public static class RegistryInterop
    {
        private const string GenshinKey = @"HKEY_CURRENT_USER\Software\miHoYo\原神";
        private const string SdkKey = "MIHOYOSDK_ADL_PROD_CN_h3123967166";
        private const string DataKey = "GENERAL_DATA_h2389025596";

        public static bool Set(GenshinAccount? account)
        {
            if (account?.MihoyoSDK is not null && account.GeneralData is not null)
            {
                Registry.SetValue(GenshinKey, SdkKey, Encoding.UTF8.GetBytes(account.MihoyoSDK));
                Registry.SetValue(GenshinKey, DataKey, Encoding.UTF8.GetBytes(account.GeneralData));
                return true;
            }
            return false;
        }

        /// <summary>
        /// 在注册表中获取账号信息
        /// 若不提供命名器，则返回的账号仅用于比较，不应存入列表中
        /// </summary>
        /// <param name="accountNamer"></param>
        /// <returns></returns>
        public static GenshinAccount? Get()
        {
            object? sdk = Registry.GetValue(GenshinKey, SdkKey, "");
            object? data = Registry.GetValue(GenshinKey, DataKey, "");

            if(sdk is null || data is null)
            {
                return null;
            }

            string sdkString = Encoding.UTF8.GetString((byte[])sdk);
            string dataString = Encoding.UTF8.GetString((byte[])data);

            return new GenshinAccount { MihoyoSDK = sdkString, GeneralData = dataString };
        }

        /// <summary>
        /// 在注册表中获取账号信息
        /// 若不提供命名器，则返回的账号仅用于比较，不应存入列表中
        /// </summary>
        /// <param name="accountNamer"></param>
        /// <returns></returns>
        public static async Task<GenshinAccount?> GetAsync(Func<GenshinAccount, Task<string?>> asyncAccountNamer)
        {
            object? sdk = Registry.GetValue(GenshinKey, SdkKey, null);
            object? data = Registry.GetValue(GenshinKey, DataKey, null);

            if (sdk is null || data is null)
            {
                return null;
            }

            string sdkString = Encoding.UTF8.GetString((byte[])sdk);
            string dataString = Encoding.UTF8.GetString((byte[])data);

            GenshinAccount account = new() { MihoyoSDK = sdkString, GeneralData = dataString };
            account.Name = await asyncAccountNamer.Invoke(account);
            return account;
        }
    }

}
