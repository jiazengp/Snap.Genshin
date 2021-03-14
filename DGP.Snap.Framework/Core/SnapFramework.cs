using DGP.Snap.Framework.Core.Model;

namespace DGP.Snap.Framework.Core
{
    public class SnapFramework
    {
        public void Initialize()
        {
            Singleton<LifeCycle.LifeCycle>.Instance.InitializeAll();
        }
        public void UnInitialize()
        {
            Singleton<LifeCycle.LifeCycle>.Instance.UnInitializeAll();
        }

        #region 单例
        private static SnapFramework instance;
        private static readonly object _lock = new object();
        private SnapFramework()
        {
        }
        public static SnapFramework Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new SnapFramework();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion

    }
}
