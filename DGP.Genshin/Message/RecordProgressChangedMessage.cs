namespace DGP.Genshin.Message
{
    public class RecordProgressChangedMessage : TypedMessage<string?>
    {
        public RecordProgressChangedMessage(string? value) : base(value)
        {
        }
    }
}
