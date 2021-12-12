using DGP.Genshin.Services.Settings;
using System;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface ISettingService
    {
        object? this[string key] { set; }

        event SettingChangedHandler? SettingChanged;

        bool? Equals<T>(string key, T? defaultValue, T? value) where T : IEquatable<T>;
        T? GetOrDefault<T>(string key, T defaultValue);
        T GetOrDefault<T>(string key, T defaultValue, Func<object?, T> converter);
        void Initialize();
        void SetValueNoNotify(string key, object value);
        void UnInitialize();
    }
}
