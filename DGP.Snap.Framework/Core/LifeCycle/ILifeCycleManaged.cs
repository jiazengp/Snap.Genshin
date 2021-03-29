namespace DGP.Snap.Framework.Core.LifeCycling
{
    public interface ILifeCycleManaged
    {
        /// <summary>
        /// 生命周期开始时调用
        /// </summary>
        public void Initialize();
        /// <summary>
        /// 生命周期结束时调用
        /// </summary>
        public void UnInitialize();
    }
}
