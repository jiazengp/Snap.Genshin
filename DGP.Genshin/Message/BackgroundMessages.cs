namespace DGP.Genshin.Message
{
    public class BackgroundOpacityChangedMessage : TypedMessage<double>
    {
        public BackgroundOpacityChangedMessage(double value) : base(value) { }
    }

    public class AdaptiveBackgroundOpacityChangedMessage : TypedMessage<double>
    {
        public AdaptiveBackgroundOpacityChangedMessage(double value) : base(value) { }
    }

    public class BackgroundChangeRequestMessage
    {

    }
}
