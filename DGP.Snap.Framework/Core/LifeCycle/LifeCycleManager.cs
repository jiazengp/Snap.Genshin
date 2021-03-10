using DGP.Snap.Framework.Core.Entry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Snap.Framework.Core.LifeCycle
{
    public class LifeCycleManager
    {
        private readonly List<IAutoLifeCycle> _instances=new List<IAutoLifeCycle>();

        /// <summary>
        /// 生命周期开始时调用
        /// </summary>
        public void InitializeAll()
        {
            if (_instances != null && _instances.Count >= 0)
            {
                foreach (IAutoLifeCycle instance in _instances)
                {
                    instance.Initialize();
                }
            }
            
        }
        /// <summary>
        /// 生命周期结束时调用
        /// </summary>
        public void UnInitializeAll()
        {
            if (_instances != null && _instances.Count >= 0)
            {
                foreach (IAutoLifeCycle instance in _instances)
                {
                    instance.UnInitialize();
                }
            }
        }
        #region 单例
        private static LifeCycleManager instance;
        private static readonly object _lock = new object();
        private LifeCycleManager()
        {
            foreach(Type t in EntryHelper.GetEntryTypes())
            {
                foreach(Type @interface in t.GetInterfaces())
                {
                    if (@interface == typeof(IAutoLifeCycle)&& @interface.IsClass)
                        _instances.Add((IAutoLifeCycle)Activator.CreateInstance(@interface));
                }
            }
        }
        public static LifeCycleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new LifeCycleManager();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }

    public interface IAutoLifeCycle
    {
        /// <summary>
        /// 生命周期开始时调用
        /// </summary>
        public void Initialize();
        /// <summary>
        /// 生命周期结束时调用
        /// </summary>
        public void UnInitialize();

        public IAutoLifeCycle Self { get; }
    }

}
