namespace DGP.Genshin.Message
{
    /// <summary>
    /// 表示当前的Cookie发生变化
    /// </summary>
    public class CookieChangedMessage : TypedMessage<string>
    {
        public CookieChangedMessage(string cookie) : base(cookie) { }
    }
    /// <summary>
    /// 新增Cookie
    /// </summary>
    public class CookieAddedMessage : TypedMessage<string>
    {
        public CookieAddedMessage(string cookie) : base(cookie) { }
    }
    /// <summary>
    /// 删除Cookie
    /// </summary>
    public class CookieRemovedMessage : TypedMessage<string>
    {
        public CookieRemovedMessage(string cookie) : base(cookie) { }
    }
}
