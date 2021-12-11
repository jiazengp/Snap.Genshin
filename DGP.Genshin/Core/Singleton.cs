using DGP.Genshin.Common.Exceptions;
using System;
using System.Collections.Concurrent;

namespace DGP.Genshin.Core
{
    internal static class Singleton<T> where T : class
    {
        private static readonly ConcurrentDictionary<Type,T> _instances = new();

        public static T Instance
        {
            get
            {
                _instances[typeof(T)] ??= Activator.CreateInstance(typeof(T), true) as T
                       ?? throw new UnexceptedNullException($"创建类型为 {typeof(T)} 的对象失败");
                return _instances[typeof(T)];
            }
        }
    }
}
