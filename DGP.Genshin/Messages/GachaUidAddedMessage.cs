using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace DGP.Genshin.Messages
{
    public class GachaUidAddedMessage : ValueChangedMessage<string>
    {
        public GachaUidAddedMessage(string uid) : base(uid) { }
    }
}
