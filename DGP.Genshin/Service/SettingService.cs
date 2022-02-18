using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Messaging;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Data.Json;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace DGP.Genshin.Service
{
    /// <summary>
    /// 设置服务的默认实现
    /// </summary>
    [Service(typeof(ISettingService), InjectAs.Singleton)]
    internal class SettingService : ISettingService
    {
        private readonly string settingFile = PathContext.Locate("settings.json");

        private ConcurrentDictionary<string, object?> settings = new();

        public T? GetOrDefault<T>(string key, T? defaultValue)
        {
            if (!settings.TryGetValue(key, out object? value))
            {
                settings[key] = defaultValue;
                return defaultValue;
            }
            else
            {
                return (T?)value;
            }
        }

        public T GetOrDefault<T>(string key, T defaultValue, Func<object, T> converter)
        {
            if (!settings.TryGetValue(key, out object? value))
            {
                settings[key] = defaultValue;
                return defaultValue;
            }
            else
            {
                return converter.Invoke(value!);
            }
        }

        public T? GetComplexOrDefault<T>(string key, T? defaultValue) where T : class
        {
            if (!settings.TryGetValue(key, out object? value))
            {
                settings[key] = defaultValue;
                return defaultValue;
            }
            else
            {
                return value is null ? null : Json.ToObject<T>(value.ToString()!);
            }
        }

        public object? this[string key]
        {
            set
            {
                settings[key] = value;
                App.Messenger.Send(new SettingChangedMessage(key, value));
            }
        }

        public void SetValueNoNotify(string key, object value,bool log)
        {
            if (log)
            {
                this.Log($"setting {key} to {value} internally without notify");
            }
            settings[key] = value;
        }

        public void Initialize()
        {
            if (File.Exists(settingFile))
            {
                settings = Json.ToObjectOrNew<ConcurrentDictionary<string, object?>>(File.ReadAllText(settingFile));
            }
        }

        public void UnInitialize()
        {
            Json.ToFile(settingFile, settings);
        }
    }
}
