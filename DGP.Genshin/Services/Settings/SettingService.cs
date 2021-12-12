using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Common.Extensions.System.Collections.Generic;
using DGP.Genshin.Services.Abstratcions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DGP.Genshin.Services.Settings
{
    /// <summary>
    /// 实现了各项设置的保存
    /// </summary>
    [Service(typeof(ISettingService),ServiceType.Singleton)]
    internal class SettingService : ISettingService
    {
        private const string settingsFileName = "settings.json";
        private readonly string settingFile = AppDomain.CurrentDomain.BaseDirectory + settingsFileName;

        private Dictionary<string, object?> settingDictionary = new();

        public T? GetOrDefault<T>(string key, T defaultValue)
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
        public T GetOrDefault<T>(string key, T defaultValue, Func<object?, T> converter)
        {
            if (!settingDictionary.TryGetValue(key, out object? value))
            {
                settingDictionary[key] = defaultValue;
                return defaultValue;
            }
            else
            {
                return converter.Invoke(value);
            }
        }
        public bool? Equals<T>(string key, T? defaultValue, T? value) where T : IEquatable<T>
        {
            return GetOrDefault(key, defaultValue)?.Equals(value);
        }
        public object? this[string key]
        {
            set
            {
                settingDictionary[key] = value;
                SettingChanged?.Invoke(key, value);
            }
        }

        /// <summary>
        /// 设置设置选项，不触发改变事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValueNoNotify(string key, object value)
        {
            this.Log($"setting {key} to {value} internally without notify");
            settingDictionary[key] = value;
        }

        /// <summary>
        /// 当设置项发生改变时触发
        /// </summary>
        public event SettingChangedHandler? SettingChanged;

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
