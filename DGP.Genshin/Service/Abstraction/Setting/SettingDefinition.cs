using System;
using System.Diagnostics.CodeAnalysis;

namespace DGP.Genshin.Service.Abstraction.Setting
{
    /// <summary>
    /// 设置入口定义
    /// </summary>
    /// <typeparam name="T">设置项定义</typeparam>
    public class SettingDefinition<T>
    {
        public static readonly ISettingService settingService;
        static SettingDefinition()
        {
            settingService = App.AutoWired<ISettingService>();
        }

        public SettingDefinition(string name, T defaultValue, Func<object, T>? converter = null)
        {
            Name = name;
            DefaultValue = defaultValue;
            Converter = converter;
        }

        public string Name { get; }
        public T DefaultValue { get; }
        public Func<object, T>? Converter { get; }

        public T Get()
        {
            return settingService.Get(this);
        }

        [return: NotNull]
        public T GetNonValueType(Func<T> defaultValueFactory)
        {
            T obj = Get();
            if (obj is null)
            {
                obj = defaultValueFactory.Invoke()!;
                Set(obj);
            }
            return obj;
        }

        public void Set(object? value, bool log = false)
        {
            settingService.Set(this, value, log);
        }

        /// <summary>
        /// 提供单参数重载以便 <see cref="Snap.Core.Mvvm.ObservableObject2"/> 的通知方法调用
        /// </summary>
        /// <param name="value"></param>
        public void Set(T value)
        {
            Set(value, false);
        }

        public static implicit operator T(SettingDefinition<T> me)
        {
            return me.Get();
        }
    }
}
