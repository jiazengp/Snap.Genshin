namespace DGP.Genshin.Services.GachaStatistic
{
    public class FetchProgress
    {
        public string Type;
        public int Page;
        public override string ToString() => $"{this.Type} 第 {this.Page} 页";
    }
}
