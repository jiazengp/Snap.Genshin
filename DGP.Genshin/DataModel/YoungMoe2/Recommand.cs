using DGP.Genshin.DataModel.Characters;
using System.Collections.Generic;

namespace DGP.Genshin.DataModel.YoungMoe2
{
    /// <summary>
    /// 推荐的配队
    /// </summary>
    public class Recommand
    {
        public List<Character?>? UpHalf { get; set; }
        public List<Character?>? DownHalf { get; set; }

        public int Count { get; set; }
    }
}
