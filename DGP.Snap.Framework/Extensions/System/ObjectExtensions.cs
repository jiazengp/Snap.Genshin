using DGP.Snap.Framework.Core.Logging;
using System;
using System.Diagnostics;

namespace DGP.Snap.Framework.Extensions.System
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="obj">记录日志的调用类</param>
        /// <param name="info">要记录入日志的信息</param>
        /// <param name="formatter">格式化输入处理函数</param>
        public static void Log<T>(this T obj, object info, Func<object, string> formatter = null) =>
            Logger.Instance.LogInternal<T>(info, formatter);
    }
}
