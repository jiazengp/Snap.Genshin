namespace DGP.Genshin.Service.Abstraction.Setting
{
    /// <summary>
    /// 设置服务
    /// 否则会影响已有的设置值
    /// </summary>
    public interface ISettingService
    {
        /// <summary>
        /// 使用定义获取设置值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definition"></param>
        /// <returns></returns>
        T Get<T>(SettingDefinition<T> definition);

        /// <summary>
        /// 初始化设置服务，加载设置数据
        /// </summary>
        void Initialize();

        /// <summary>
        /// 使用定义设置设置值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definition"></param>
        /// <param name="value"></param>
        /// <param name="notify"></param>
        /// <param name="log"></param>
        void Set<T>(SettingDefinition<T> definition, object? value, bool log = false);

        /// <summary>
        /// 卸载设置数据
        /// </summary>
        void UnInitialize();
    }
}
