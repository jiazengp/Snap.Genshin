using DGP.Snap.Framework.Core.Entry;
using DGP.Snap.Framework.Extensions.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Snap.Framework.Core.LifeCycling
{
    public class LifeCycle
    {
        private static readonly List<object> _instances = new();
        private bool initalized = false;
        /// <summary>
        /// 生命周期开始
        /// </summary>
        public void InitializeAll()
        {
            this.Log($"Searching among {EntryHelper.GetCurrentTypes().Count()} types");
            foreach (Type type in EntryHelper.GetCurrentTypes())
            {
                foreach (Type @interface in type.GetInterfaces())
                {
                    if (@interface == typeof(ILifeCycleManaged) && type.IsClass)
                    {
                        _instances.Add(Activator.CreateInstance(type));
                        this.Log($"{type} added to auto lifecycle manager");
                    }
                }
            }

            if (this.initalized)
                return;
            if (_instances != null && _instances.Count >= 0)
            {
                foreach (ILifeCycleManaged instance in _instances)
                {
                    instance.Initialize();
                }
            }
            this.initalized = true;
        }
        /// <summary>
        /// 生命周期结束
        /// </summary>
        public void UnInitializeAll()
        {
            if (_instances != null && _instances.Count >= 0)
            {
                foreach (ILifeCycleManaged instance in _instances)
                {
                    instance.UnInitialize();
                }
            }
        }

        public static T InstanceOf<T>() where T : ILifeCycleManaged
        {
            object ins = _instances.Find(t => t.GetType() == typeof(T));
            if (ins == null)
            {
                object o = Activator.CreateInstance(typeof(T));
                _instances.Add(o);
                return (T)o;
            }
            return (T)ins;
        }
        internal static object InstanceOf(Type type) => _instances.Find(t => t.GetType() == type);
    }
}
