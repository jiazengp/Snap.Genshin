namespace DGP.Genshin.Service.Abstraction.GachaStatistic
{
    /// <summary>
    /// 获取抽卡Url的模式
    /// </summary>
    public enum GachaLogUrlMode
    {
        /// <summary>
        /// 从游戏日志获取
        /// </summary>
        GameLogFile,

        /// <summary>
        /// 手动输入
        /// </summary>
        ManualInput,
    }
}