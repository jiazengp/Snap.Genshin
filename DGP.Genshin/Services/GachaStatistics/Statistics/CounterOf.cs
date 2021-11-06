using System.Collections.Generic;

namespace DGP.Genshin.Services.GachaStatistics.Statistics
{
    /// <summary>
    /// 表示一个对 <see cref="T"/> 类型的计数器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CounterOf<T> : Dictionary<string, T> { }
}