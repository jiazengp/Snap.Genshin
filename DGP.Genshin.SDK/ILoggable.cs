using System.Runtime.CompilerServices;

namespace DGP.Genshin.SDK
{
    /// <summary>
    /// 实现日志输出
    /// </summary>
    public interface ILoggable {
        event LogHandler Log;
    }

    public delegate void LogHandler(object info, Func<object, string>? formatter = null,
    [CallerMemberName] string? callerMemberName = null,
    [CallerLineNumber] int? callerLineNumber = null,
    [CallerFilePath] string? callerFilePath = null);
}