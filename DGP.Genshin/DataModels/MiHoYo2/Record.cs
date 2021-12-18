using DGP.Genshin.MiHoYoAPI.Record;
using DGP.Genshin.MiHoYoAPI.Record.Avatar;
using DGP.Genshin.MiHoYoAPI.Record.SpiralAbyss;
using System.Collections.Generic;

namespace DGP.Genshin.DataModels.MiHoYo2
{
    /// <summary>
    /// 包装一次查询的数据
    /// </summary>
    public class Record
    {
        /// <summary>
        /// 构造新的<see cref="Record"/>实例
        /// </summary>
        public Record()
        {
        }

        /// <summary>
        /// 构造新的<see cref="Record"/>实例
        /// </summary>
        /// <param name="message"></param>
        public Record(string? message)
        {
            Message = message;
        }

        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? UserId { get; set; }
        public string? Server { get; set; }

        public PlayerInfo? PlayerInfo { get; set; }
        public SpiralAbyss? SpiralAbyss { get; set; }
        public SpiralAbyss? LastSpiralAbyss { get; set; }
        public List<DetailedAvatar>? DetailedAvatars { get; set; }
    }
}