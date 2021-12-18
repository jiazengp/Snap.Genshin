namespace DGP.Genshin.Messages
{
    public class GachaUidAddedMessage : TypedMessage<string>
    {
        public GachaUidAddedMessage(string uid) : base(uid) { }
    }
}
