namespace DGP.Genshin.Message
{
    public class UpdateProgressedMessage
    {
        private static UpdateProgressedMessage? defaultValue;
        public static UpdateProgressedMessage Default
        {
            get
            {
                defaultValue ??= new UpdateProgressedMessage(0, string.Empty, false);
                return defaultValue;
            }
        }


        public UpdateProgressedMessage(double value, string valueString, bool isDownloading)
        {
            Value = value;
            ValueString = valueString;
            IsDownloading = isDownloading;
        }

        public double Value { get; }
        public string ValueString { get; }
        public bool IsDownloading { get; }
    }

}
