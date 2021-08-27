using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using DGP.Snap.Framework.Extensions.System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;

namespace DGP.Genshin.Services.Settings
{
    /// <summary>
    /// 实现了各项设置的保存，向下兼容
    /// </summary>
    internal class SettingService
    {
        private const string settingsFileName = "settings.json";
        private readonly string settingFile = AppDomain.CurrentDomain.BaseDirectory + settingsFileName;

        private Dictionary<string, object> settingDictionary = new();

        public T GetOrDefault<T>(string key, T defaultValue)
        {
            if (!this.settingDictionary.TryGetValue(key, out object value))
            {
                this.settingDictionary.AddOrSet(key, defaultValue);
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
                this.settingDictionary.AddOrSet(key, defaultValue);
                return defaultValue;
            }
            else
            {
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
        /// <summary>
        /// 设置设置选项，不触发改变事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void SetValueInternal(string key, object value) => this.settingDictionary.AddOrSet(key, value);
        /// <summary>
        /// 当设置项发生改变时触发
        /// </summary>
        public event SettingChangedHandler SettingChanged;

        public void Initialize()
        {
            if (File.Exists(this.settingFile))
            {
                string json;
                using (StreamReader sr = new(this.settingFile))
                {
                    json = sr.ReadToEnd();
                }
                this.settingDictionary = Json.ToObject<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
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
            this.Log("initialized");
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
    /// <summary>
    /// 设置项改变委托
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public delegate void SettingChangedHandler(string key, object value);
}
