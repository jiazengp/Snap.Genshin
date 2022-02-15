namespace DGP.Genshin.DataModel.HutaoAPI
{
    /// <summary>
    /// Id Name Icon Value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Item<T>
    {
        public Item(int id, string? name, string? icon, T value)
        {
            Id = id;
            Name = name;
            Icon = icon;
            Value = value;
        }

        public int Id { get; init; }
        public string? Name { get; init; }
        public string? Icon { get; init; }
        public T Value { get; init; }
    }
}
