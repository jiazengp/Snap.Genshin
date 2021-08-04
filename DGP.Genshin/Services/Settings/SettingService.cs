using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;

namespace DGP.Genshin.Services.Settings
{
    internal class SettingService
    {
        private const string settingsFileName = "settings.json";
        private readonly string settingFile = AppDomain.CurrentDomain.BaseDirectory + settingsFileName;

        private Dictionary<string, object> settingDictionary = new();

        public T GetOrDefault<T>(string key, T defaultValue)
        {
            if (!this.settingDictionary.TryGetValue(key, out object value))
            {
                return defaultValue;
            }
            else
            {
                this.settingDictionary.AddOrSet(key, value);
                return (T)value;
            }
        }
        public T GetOrDefault<T>(string key, T defaultValue, Func<object, T> converter)
        {

            if (!this.settingDictionary.TryGetValue(key, out object value))
            {
                return defaultValue;
            }
            else
            {
                this.settingDictionary.AddOrSet(key, value);
                return converter.Invoke(value);
            }
        }

        public bool Has(string key) => this.settingDictionary.ContainsKey(key);

        public object this[string key]
        {
            set
            {
                this.settingDictionary.AddOrSet(key, value);
                SettingChanged?.Invoke(key, value);
            }
        }

        public static event Action<string, object> SettingChanged;

        public void Initialize()
        {
            if (File.Exists(this.settingFile))
            {
                string json;
                using (StreamReader sr = new(this.settingFile))
                {
                    json = sr.ReadToEnd();
                }
                Dictionary<string, object> dict = Json.ToObject<Dictionary<string, object>>(json);
                if (dict != null)
                {
                    this.settingDictionary = dict;
                    return;
                }
            }
            File.Create(this.settingFile).Dispose();
            this.settingDictionary = new Dictionary<string, object>();
        }
        public void UnInitialize()
        {
            string json = Json.Stringify(this.settingDictionary);
            using StreamWriter sw = new StreamWriter(File.Create(this.settingFile));
            sw.Write(json);
        }


        #region 单例
        private static SettingService instance;
        private static readonly object _lock = new();
        private SettingService()
        {
        }
        public static SettingService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new SettingService();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
