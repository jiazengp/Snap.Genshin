using Snap.Net.Download;

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

        public UpdateProgressedMessage(DownloadInfomation infomation)
        {
            Value = infomation.Percent;
            ValueString = infomation.ToString();
            IsDownloading = infomation.IsDownloading;
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
