namespace DGP.Genshin.Message
{
    public class BackgroundOpacityChangedMessage : TypedMessage<double> 
    {
        public BackgroundOpacityChangedMessage(double value):base(value) { }
    }
}
