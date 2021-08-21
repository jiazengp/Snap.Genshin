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

        /// <summary>
        /// 初始化源为目标值，当源存在值时跳过
        /// </summary>
        /// <typeparam name="T">源类型</typeparam>
        /// <param name="source">源</param>
        /// <param name="target">目标</param>
        /// <returns></returns>
        public static T InitValue<T>(this T source,T target)
        {
            if (source == null)
                source = target;
            return source;
        }
    }
}
