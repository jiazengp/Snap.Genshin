using DGP.Genshin.MiHoYoAPI.Gacha;

namespace DGP.Genshin.Services.GachaStatistics
{
    public class UidGachaData
    {
        public UidGachaData(string uid,GachaData data)
        {
            Uid = uid;
            Data = data;
        }
        public string Uid { get; set; }
        public GachaData Data { get; set; }
    }
}
