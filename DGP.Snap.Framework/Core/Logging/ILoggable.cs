namespace DGP.Snap.Framework.Core.Logging
{
    /// <summary>
    /// 使类支持日志记录
    /// 用法：
    /// <code><see cref="Log"/>?.<see cref="Invoke(object)"/></code>
    /// </summary>
    public interface ILoggable
    {
        public event LogHandler Log;
    }
}

