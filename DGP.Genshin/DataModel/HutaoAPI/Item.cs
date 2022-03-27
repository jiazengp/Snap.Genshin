namespace DGP.Genshin.DataModel.HutaoAPI
{
    /// <summary>
    /// 胡桃API数据元件：
    /// Id Name Icon Value
    /// </summary>
    /// <typeparam name="TValue">值的类型</typeparam>
    public class Item<TValue>
    {
        public Item(int id, string? name, string? icon, TValue value)
        {
            this.Id = id;
            this.Name = name;
            this.Icon = icon;
            this.Value = value;
        }

        public int Id { get; init; }
        public string? Name { get; init; }
        public string? Icon { get; init; }
        public TValue Value { get; init; }
    }
}
