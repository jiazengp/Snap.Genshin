using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System.Collections.Generic;

namespace DGP.Genshin.Messages
{
    public class SettingChangedMessage : ValueChangedMessage<KeyValuePair<string, object?>>
    {
        public SettingChangedMessage(KeyValuePair<string, object?> value) : base(value) { }
        public SettingChangedMessage(string key, object? value) : base(new(key, value)) { }
    }
}
