using DGP.Genshin.Core.Background.Abstraction;
using DGP.Genshin.Core.ImplementationSwitching;
using Newtonsoft.Json;
using Snap.Data.Json;
using Snap.Data.Utility.Extension;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DGP.Genshin.Core.Background.Xunkong
{
    [SwitchableImplementation(typeof(IBackgroundProvider), "Xunkong.Wallpaper", "寻空壁纸")]
    internal class XunkongWallpaperProvider : IBackgroundProvider
    {
        private const string Api = "https://api.xunkong.cc/v0.1/genshin/wallpaper/random";

        public async Task<BitmapImage?> GetNextBitmapImageAsync()
        {
            try
            {
                XunkongResponse<XunkongWallpaperInfo>? result = await Json.FromWebsiteAsync<XunkongResponse<XunkongWallpaperInfo>>(Api);
                if (await XunkongFileCache.HitAsync(result?.Data?.Url) is MemoryStream stream)
                {
                    BitmapImage bitmapImage = new();
                    using (bitmapImage.AsDisposableInit())
                    {
                        bitmapImage.StreamSource = stream;
                    }
                    return bitmapImage;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    internal record XunkongResponse<T>
    {
        public T? Data { get; set; }
    }

    internal record XunkongWallpaperInfo
    {
        [JsonProperty("url")] public string? Url { get; set; }
    }
}
