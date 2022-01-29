namespace DGP.Genshin.Message
{
    public class TypedMessage<T>
    {
        public T Value { get; }
        public TypedMessage(T value)
        {
            Value = value;
        }
    }
}
