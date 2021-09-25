using System.IO;
using System.Windows.Media.Imaging;

namespace DGP.Genshin.Services.Screenshots
{
    public class Screenshot
    {
        public Screenshot(string path)
        {
            this.Path = path;
        }
        public string Path { get; set; }

        private BitmapImage image;
        private readonly object locker = new object();
        public BitmapImage Image
        {
            get
            {
                lock (this.locker)
                {
                    if (this.image == null)
                    {
                        this.image = new BitmapImage();
                        this.image.BeginInit();
                        MemoryStream ms = new MemoryStream();
                        using (FileStream fs = new FileStream(this.Path, FileMode.Open, FileAccess.Read))
                        {
                            fs.CopyTo(ms);
                        }
                        this.image.StreamSource = ms;
                        this.image.CacheOption = BitmapCacheOption.OnLoad;
                        this.image.EndInit();
                    }
                    return this.image;
                }
            }
        }
        public string Name { get; set; }
    }
}
