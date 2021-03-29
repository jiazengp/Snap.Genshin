using DGP.Snap.Framework.Core.Model;

namespace DGP.Snap.Framework.Core
{
    public class SnapFramework
    {
        public void Initialize()
        {
            Singleton<LifeCycling.LifeCycle>.Instance.InitializeAll();
        }
        public void UnInitialize()
        {
            Singleton<LifeCycling.LifeCycle>.Instance.UnInitializeAll();
        }

        #region 单例
        private static SnapFramework current;
        private static readonly object _lock = new object();
        private SnapFramework()
        {
        }
        public static SnapFramework Current
        {
            get
            {
                if (current == null)
                {
                    lock (_lock)
                    {
                        if (current == null)
                        {
                            current = new SnapFramework();
                        }
                    }
                }
                return current;
            }
        }
        #endregion

    }
}
