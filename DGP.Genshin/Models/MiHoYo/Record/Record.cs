using DGP.Genshin.Models.MiHoYo.Record.Avatar;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Record
{
    internal class Record
    {
        public Record()
        {
        }
        public Record(string message)
        {
            this.Message = message;
        }

        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public string UserId { get; set; }
        public string Server { get; set; }

        public PlayerInfo PlayerInfo { get; set; }
        public SpiralAbyss.SpiralAbyss SpiralAbyss { get; set; }
        public SpiralAbyss.SpiralAbyss LastSpiralAbyss { get; set; }
        public List<DetailedAvatar> DetailedAvatars { get; set; }
        public dynamic Activities { get; set; }
    }
}
