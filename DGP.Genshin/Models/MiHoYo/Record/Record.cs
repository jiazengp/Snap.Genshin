using DGP.Genshin.Models.MiHoYo.Record.Avatar;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Record
{
    /// <summary>
    /// 包装一次查询的数据
    /// </summary>
    [InterModel]
    public class Record
    {
        public Record()
        {
        }
        /// <summary>
        /// 构造新的失败的<see cref="Record"/>实例
        /// </summary>
        /// <param name="message"></param>
        public Record(string? message)
        {
            Message = message;
        }

        public bool Success { get; set; } = false;
        public string? Message { get; set; }
        public string? UserId { get; set; }
        public string? Server { get; set; }

        public PlayerInfo? PlayerInfo { get; set; }
        public SpiralAbyss.SpiralAbyss? SpiralAbyss { get; set; }
        public SpiralAbyss.SpiralAbyss? LastSpiralAbyss { get; set; }
        public List<DetailedAvatar>? DetailedAvatars { get; set; }
        public dynamic? Activities { get; set; }
    }
}
