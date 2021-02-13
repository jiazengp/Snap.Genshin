using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DGP.Genshin.Service
{
    internal class SettingService
    {
        private const string settingsFileName = "settings.json";
        private readonly string settingFile = AppDomain.CurrentDomain.BaseDirectory + settingsFileName;

        private Dictionary<string, object> settingDictionary = new Dictionary<string, object>();

        public T GetOrDefault<T>(string key, T defaultValue)
        {
            if (this.settingDictionary.TryGetValue(key, out object value))
            {
                return (T)value;
            }
            else
            {
                return defaultValue;
            }
        }
        public T GetOrDefault<T>(string key, T defaultValue, Func<object, T> converter)
        {
            if (this.settingDictionary.TryGetValue(key, out object value))
            {
                return converter.Invoke(value);
            }
            else
            {
                return defaultValue;
            }
        }

        public object this[string key]
        {
            set => this.settingDictionary[key] = value;
        }

        private void Load()
        {
            if (File.Exists(this.settingFile))
            {
                string json;
                using (StreamReader sr = new StreamReader(this.settingFile))
                {
                    json = sr.ReadToEnd();
                }
                this.settingDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }
            else
            {
                File.Create(this.settingFile).Dispose();
                this.settingDictionary = new Dictionary<string, object>();
            }
        }

        public void Unload()
        {
            if (!File.Exists(this.settingFile))
            {
                File.Create(this.settingFile).Dispose();
            }

            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented
            };
            string json = JsonConvert.SerializeObject(this.settingDictionary, jsonSerializerSettings);

            using (StreamWriter sw = new StreamWriter(this.settingFile))
            {
                sw.Write(json);
            }
        }

        #region 单例
        private static SettingService instance;
        private static readonly object _lock = new object();
        private SettingService() => this.Load();
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
