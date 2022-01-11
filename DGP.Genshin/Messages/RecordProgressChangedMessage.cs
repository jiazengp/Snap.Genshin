namespace DGP.Genshin.Messages
{
    public class RecordProgressChangedMessage : TypedMessage<string?>
    {
        public RecordProgressChangedMessage(string? value) : base(value)
        {
        }
    }
}
