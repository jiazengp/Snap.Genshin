namespace DGP.Genshin.DataModel.HutaoAPI
{
    public class Item<T>
    {
        public Item(int id, string? name, string? icon, T value)
        {
            Id = id;
            Name = name;
            Icon = icon;
            Value = value;
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Icon { get; set; }
        public T Value { get; set; }
    }
}
