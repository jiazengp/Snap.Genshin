namespace DGP.Genshin.Message
{
    /// <summary>
    /// 为类型消息提供基类
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public class TypedMessage<T>
    {
        public T Value { get; }
        public TypedMessage(T value)
        {
            this.Value = value;
        }
    }
}
