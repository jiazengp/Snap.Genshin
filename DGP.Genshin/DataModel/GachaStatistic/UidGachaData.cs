using DGP.Genshin.MiHoYoAPI.Gacha;

namespace DGP.Genshin.DataModel.GachaStatistic
{
    public class UidGachaData
    {
        public UidGachaData(string uid, GachaData data)
        {
            this.Uid = uid;
            this.Data = data;
        }
        public string Uid { get; set; }
        public GachaData Data { get; set; }
    }
}
