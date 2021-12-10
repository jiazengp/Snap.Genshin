namespace DGP.Genshin.SDK
{
    public interface IPlugin
    {
    }

    public delegate void Log(object info, Func<object, string>? formatter = null,
            string? callerMemberName = null, int? callerLineNumber = null, string? callerFilePath = null);
}