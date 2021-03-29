using DGP.Snap.Framework.Core.LifeCycling;
using System;
using System.Collections.Concurrent;

namespace DGP.Snap.Framework.Core.Model
{
    public static class Singleton<T> where T : new()
    {
        internal static readonly ConcurrentDictionary<Type, T> _instances = new();

        public static T Instance
        {
            get
            {
                if (typeof(T) == typeof(ILifeCycleManaged))
                    return (T)LifeCycling.LifeCycle.InstanceOf(typeof(T));
                return _instances.GetOrAdd(typeof(T), (t) => new T());
            }
        }
    }
}
