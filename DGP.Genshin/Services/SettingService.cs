using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Messages;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.IO;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// 设置服务的默认实现
    /// </summary>
    [Service(typeof(ISettingService), ServiceType.Singleton)]
    [Send(typeof(SettingChangedMessage))]
    internal class SettingService : ISettingService
    {
        private const string settingsFileName = "settings.json";
        private readonly string settingFile = Path.Combine(AppContext.BaseDirectory, settingsFileName);

        private Dictionary<string, object?> settingDictionary = new();

        public object? GetBoxedOrDefault(string key, object defaultValue)
        {
            if (!settingDictionary.TryGetValue(key, out object? value))
            {
                settingDictionary[key] = defaultValue;
                return defaultValue;
            }
            else
            {
                //won't be null if TryGetValue return true.
                return value!;
            }
        }

        public T? GetOrDefault<T>(string key, T? defaultValue)
        {
            if (!settingDictionary.TryGetValue(key, out object? value))
            {
                settingDictionary[key] = defaultValue;
                return defaultValue;
            }
            else
            {
                return (T?)value;
            }
        }

        public T GetOrDefault<T>(string key, T defaultValue, Func<object, T> converter)
        {
            if (!settingDictionary.TryGetValue(key, out object? value))
            {
                settingDictionary[key] = defaultValue;
                return defaultValue;
            }
            else
            {
                return converter.Invoke(value!);
            }
        }

        public object? this[string key]
        {
            set
            {
                settingDictionary[key] = value;
                App.Messenger.Send(new SettingChangedMessage(key, value));
            }
        }

        public void SetValueNoNotify(string key, object value)
        {
            this.Log($"setting {key} to {value} internally without notify");
            settingDictionary[key] = value;
        }

        public void Initialize()
        {
            if (File.Exists(settingFile))
            {
                string json;
                using (StreamReader sr = new(settingFile))
                {
                    json = sr.ReadToEnd();
                }
                settingDictionary = Json.ToObject<Dictionary<string, object?>>(json) ?? new Dictionary<string, object?>();
            }
            this.Log("initialized");
        }

        public void UnInitialize()
        {
            string json = Json.Stringify(settingDictionary);
            using StreamWriter sw = new(File.Create(settingFile));
            sw.Write(json);
        }
    }
}
