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
            this.Value = infomation.Percent;
            this.ValueString = infomation.ToString();
            this.IsDownloading = infomation.IsDownloading;
        }
        public UpdateProgressedMessage(double value, string valueString, bool isDownloading)
        {
            this.Value = value;
            this.ValueString = valueString;
            this.IsDownloading = isDownloading;
        }

        public double Value { get; }
        public string ValueString { get; }
        public bool IsDownloading { get; }
    }

}
