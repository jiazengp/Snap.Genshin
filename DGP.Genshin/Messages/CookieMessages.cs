using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace DGP.Genshin.Messages
{
    /// <summary>
    /// 表示当前的Cookie发生变化
    /// </summary>
    public class CookieChangedMessage : ValueChangedMessage<string>
    {
        public CookieChangedMessage(string cookie) : base(cookie) { }
    }
    /// <summary>
    /// 新增Cookie
    /// </summary>
    public class CookieAddedMeaasge : ValueChangedMessage<string>
    {
        public CookieAddedMeaasge(string cookie) : base(cookie) { }
    }
    /// <summary>
    /// 删除Cookie
    /// </summary>
    public class CookieRemovedMessage : ValueChangedMessage<string>
    {
        public CookieRemovedMessage(string cookie) : base(cookie) { }
    }
}
