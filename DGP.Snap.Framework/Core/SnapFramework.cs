using DGP.Snap.Framework.Core.LifeCycling;
using DGP.Snap.Framework.Core.Logging;
using DGP.Snap.Framework.Core.Model;

namespace DGP.Snap.Framework.Core
{
    public class SnapFramework
    {
        public void Initialize() => Singleton<LifeCycle>.Instance.InitializeAll();
        public void UnInitialize()
        {
            Singleton<LifeCycle>.Instance.UnInitializeAll();
            Singleton<Logger>.Instance.UnInitialize();
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
