using DGP.Snap.Framework.Core.LifeCycling;
using DGP.Snap.Framework.Core.Model;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;

namespace DGP.Genshin.Services
{
    internal class SettingService : ILifeCycleManaged
    {
        private const string settingsFileName = "settings.json";
        private readonly string settingFile = AppDomain.CurrentDomain.BaseDirectory + settingsFileName;

        private Dictionary<string, object> settingDictionary = new();

        public SettingService Instance => Singleton<SettingService>.Instance;

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
            set => this.settingDictionary.AddOrSet(key, value);
        }

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
            if (!File.Exists(this.settingFile))
            {
                File.Create(this.settingFile).Dispose();
            }
            string json = Json.Stringify(this.settingDictionary);
            using StreamWriter sw = new StreamWriter(this.settingFile);
            sw.Write(json);
        }
    }
}
