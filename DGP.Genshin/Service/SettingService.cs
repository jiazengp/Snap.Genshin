using DGP.Genshin.Helper;
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
            if (!this.settingDictionary.TryGetValue(key, out object value)) 
            {
                return defaultValue;
            }
            else
            {
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
                return converter.Invoke(value);
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

        public void Unload()
        {
            if (!File.Exists(this.settingFile))
            {
                File.Create(this.settingFile).Dispose();
            }
            string json = Json.Stringify(this.settingDictionary);
            using StreamWriter sw = new StreamWriter(this.settingFile);
            sw.Write(json);
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
