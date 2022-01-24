using System.Collections.Generic;

namespace DGP.Genshin.Message
{
    public class SettingChangedMessage : TypedMessage<KeyValuePair<string, object?>>
    {
        public SettingChangedMessage(KeyValuePair<string, object?> value) : base(value) { }
        public SettingChangedMessage(string key, object? value) : base(new(key, value)) { }
    }
}
